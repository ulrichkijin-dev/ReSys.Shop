from fastapi import FastAPI, File, UploadFile, Depends, HTTPException, Query
from fastapi.middleware.cors import CORSMiddleware
from fastapi.security import APIKeyHeader
from sqlalchemy.orm import Session
from sqlalchemy import func
from typing import List, Dict
import io
import uuid
import logging
from datetime import datetime
from PIL import Image

from app.config import settings
from app.database import get_db, ProductImage, Product, Taxon, ProductClassification
from app.model_factory import model_manager
from app.model_mapping import get_embedding_prefix, get_embedding_column
from app.schemas import (
    SearchResponse,
    SearchResultItem,
    RecommendationResponse,
    EvaluationResponse,
    CategoryMetric,
    HealthResponse,
    ModelStatus,
    ModelsListResponse,
    EmbeddingGenerationRequest,
    EmbeddingGenerationResponse,
    EmbeddingDetail,
)
from app.services.search_service import SearchService

# Configure logging
logging.basicConfig(
    level=logging.INFO, format="[%(asctime)s] %(levelname)s - %(name)s - %(message)s"
)
logger = logging.getLogger(__name__)

app = FastAPI(
    title=settings.API_TITLE,
    version=settings.API_VERSION,
    description=settings.API_DESCRIPTION,
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# --- Security ---
api_key_header = APIKeyHeader(name=settings.API_KEY_NAME, auto_error=True)


async def get_api_key(api_key: str = Depends(api_key_header)):
    """Validate API key for protected endpoints."""
    if api_key != settings.API_KEY:
        raise HTTPException(status_code=403, detail="Invalid API Key")
    return api_key


# --- Lifecycle ---


@app.on_event("startup")
async def startup_event():
    """Warm up champion models on startup."""
    logger.info("🚀 Starting API initialization...")
    logger.info(f"Champion Models: {settings.AVAILABLE_MODELS}")
    model_manager.warmup(settings.AVAILABLE_MODELS)
    logger.info(
        f"✅ API ready with {len(model_manager.get_loaded_models())} models loaded"
    )


# --- Endpoints ---


@app.get("/", tags=["General"])
async def root():
    """Root endpoint with API information."""
    return {
        "service": settings.API_TITLE,
        "version": settings.API_VERSION,
        "active_model": settings.DEFAULT_MODEL,
        "available_models": settings.AVAILABLE_MODELS,
        "loaded_models": list(model_manager.get_loaded_models().keys()),
    }


@app.get("/health", response_model=HealthResponse, tags=["General"])
async def health_check(db: Session = Depends(get_db)):
    """Comprehensive health check including model status."""
    try:
        count = db.query(ProductImage).count()
        model_info = SearchService.get_model_info()

        model_statuses = [
            ModelStatus(
                model_name=name,
                architecture=info["architecture"],
                loaded=info["loaded"],
                dimensions=info["dimensions"],
            )
            for name, info in model_info.items()
        ]

        return HealthResponse(
            status="healthy",
            database="connected",
            indexed_images=count,
            models=model_statuses,
        )
    except Exception as e:
        logger.error(f"Health check failed: {e}")
        raise HTTPException(status_code=503, detail=str(e))


@app.get(
    "/models",
    response_model=ModelsListResponse,
    dependencies=[Depends(get_api_key)],
    tags=["Models"],
)
async def list_models():
    """List all available models and their status."""
    model_info = SearchService.get_model_info()

    models_dict = {
        name: ModelStatus(
            model_name=name,
            architecture=info["architecture"],
            loaded=info["loaded"],
            dimensions=info["dimensions"],
        )
        for name, info in model_info.items()
    }

    return ModelsListResponse(
        total_models=len(models_dict),
        default_model=settings.DEFAULT_MODEL,
        models=models_dict,
    )


@app.post(
    "/embeddings/generate",
    response_model=EmbeddingGenerationResponse,
    dependencies=[Depends(get_api_key)],
    tags=["Embeddings"],
)
async def generate_embeddings_batch(
    request: EmbeddingGenerationRequest, db: Session = Depends(get_db)
):
    """Batch generate embeddings for specified images."""
    if not request.image_ids:
        return EmbeddingGenerationResponse(successful=0, failed=0, details=[])

    successful, failed, details = 0, 0, []

    for image_id_str in request.image_ids:
        try:
            image_id = uuid.UUID(image_id_str)
            image = db.query(ProductImage).filter(ProductImage.id == image_id).first()

            if not image:
                details.append(
                    EmbeddingDetail(
                        image_id=image_id_str,
                        status="failed",
                        updated_embeddings=[],
                        error="Image not found",
                    )
                )
                failed += 1
                continue

            updated = []
            for m_name in settings.AVAILABLE_MODELS:
                embedder = model_manager.get_embedder(m_name)
                if not embedder:
                    continue

                # Use centralized mapping
                prefix = get_embedding_prefix(m_name)
                attr_name = f"embedding_{prefix}"

                if getattr(image, attr_name) is None:
                    emb = embedder.extract_features(image.url)
                    if emb:
                        setattr(image, attr_name, emb)
                        setattr(image, f"{attr_name}_model", embedder.name)
                        setattr(image, f"{attr_name}_generated_at", datetime.utcnow())
                        updated.append(m_name)

            db.commit()
            details.append(
                EmbeddingDetail(
                    image_id=image_id_str, status="success", updated_embeddings=updated
                )
            )
            successful += 1

        except Exception as e:
            logger.error(f"Error generating embeddings for {image_id_str}: {e}")
            details.append(
                EmbeddingDetail(
                    image_id=image_id_str,
                    status="failed",
                    updated_embeddings=[],
                    error=str(e),
                )
            )
            failed += 1
            db.rollback()

    return EmbeddingGenerationResponse(
        successful=successful, failed=failed, details=details
    )


@app.post(
    "/search/by-upload",
    response_model=SearchResponse,
    dependencies=[Depends(get_api_key)],
    tags=["Search"],
)
async def search_by_upload(
    file: UploadFile = File(...),
    limit: int = Query(10, ge=1, le=100),
    model: str = Query(settings.DEFAULT_MODEL, enum=settings.AVAILABLE_MODELS),
    db: Session = Depends(get_db),
):
    """Search for similar products by uploading an image."""
    try:
        if not file.content_type or not file.content_type.startswith("image/"):
            raise HTTPException(status_code=400, detail="File must be an image")

        contents = await file.read()
        embedder = model_manager.get_embedder(model)
        if not embedder:
            raise HTTPException(
                status_code=503, detail=f"Model '{model}' is not available."
            )

        vector = embedder.extract_features(contents)
        if vector is None:
            raise HTTPException(
                status_code=500, detail="Failed to extract image features"
            )

        results = SearchService.search_by_vector(db, vector, model, limit)
        return SearchResponse(model=model, count=len(results), results=results)

    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Search upload error: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"Search failed: {str(e)}")


@app.get(
    "/search/by-id/{image_id}",
    response_model=SearchResponse,
    dependencies=[Depends(get_api_key)],
    tags=["Search"],
)
async def search_by_image_id(
    image_id: uuid.UUID,
    limit: int = Query(10, ge=1, le=100),
    model: str = Query(settings.DEFAULT_MODEL, enum=settings.AVAILABLE_MODELS),
    db: Session = Depends(get_db),
):
    """Search for similar products using an existing image ID."""
    source_image = db.query(ProductImage).filter(ProductImage.id == image_id).first()

    if not source_image:
        raise HTTPException(status_code=404, detail="Source image not found")

    # Use centralized mapping
    prefix = get_embedding_prefix(model)
    emb_col = f"embedding_{prefix}"
    vector = getattr(source_image, emb_col)

    if vector is None:
        raise HTTPException(
            status_code=400,
            detail=f"Embedding for model '{model}' missing. Generate embeddings first.",
        )

    results = SearchService.search_by_vector(
        db, vector, model, limit, exclude_image_id=image_id
    )
    return SearchResponse(model=model, count=len(results), results=results)


@app.get(
    "/recommendations/by-product-id/{product_id}",
    response_model=RecommendationResponse,
    dependencies=[Depends(get_api_key)],
    tags=["Recommendations"],
)
async def recommendations_by_product(
    product_id: uuid.UUID,
    limit: int = Query(10, ge=1, le=100),
    model: str = Query(settings.DEFAULT_MODEL, enum=settings.AVAILABLE_MODELS),
    db: Session = Depends(get_db),
):
    """Get product recommendations based on visual similarity."""
    source_image = (
        db.query(ProductImage)
        .filter(ProductImage.product_id == product_id, ProductImage.type == "Search")
        .first()
    )

    if not source_image:
        raise HTTPException(
            status_code=404, detail="No search image found for this product"
        )

    # Use centralized mapping
    prefix = get_embedding_prefix(model)
    emb_col = f"embedding_{prefix}"
    vector = getattr(source_image, emb_col)

    if vector is None:
        raise HTTPException(
            status_code=400, detail=f"Embedding for model '{model}' missing."
        )

    results = SearchService.search_by_vector(
        db, vector, model, limit, exclude_product_id=product_id
    )
    return RecommendationResponse(
        source_product_id=product_id, model=model, count=len(results), results=results
    )


@app.get(
    "/diagnostics/compare-models/{image_id}",
    dependencies=[Depends(get_api_key)],
    tags=["Diagnostics"],
)
async def compare_models(
    image_id: uuid.UUID,
    models: List[str] = Query(settings.AVAILABLE_MODELS),
    limit: int = Query(5, ge=1, le=20),
    db: Session = Depends(get_db),
):
    """Compare search results across multiple models for diagnostics."""
    source_image = db.query(ProductImage).filter(ProductImage.id == image_id).first()

    if not source_image:
        raise HTTPException(status_code=404, detail="Source image not found")

    results_by_model = {}

    for model in models:
        prefix = get_embedding_prefix(model)
        emb_col = f"embedding_{prefix}"
        vector = getattr(source_image, emb_col)

        if vector is None:
            results_by_model[model] = {
                "model": model,
                "count": 0,
                "results": [],
                "error": "Embedding not available",
            }
            continue

        try:
            results = SearchService.search_by_vector(
                db, vector, model, limit, exclude_image_id=image_id
            )
            results_by_model[model] = {
                "model": model,
                "count": len(results),
                "results": results,
            }
        except Exception as e:
            logger.error(f"Error comparing model {model}: {e}")
            results_by_model[model] = {
                "model": model,
                "count": 0,
                "results": [],
                "error": str(e),
            }

    return {"source_image_id": str(image_id), "results_by_model": results_by_model}


@app.get(
    "/evaluation/metrics",
    response_model=EvaluationResponse,
    dependencies=[Depends(get_api_key)],
    tags=["Thesis"],
)
async def get_evaluation_metrics(
    model: str = Query(settings.DEFAULT_MODEL, enum=settings.AVAILABLE_MODELS),
    sample_size: int = Query(20, ge=1, le=100),
    db: Session = Depends(get_db),
):
    """
    Calculate thesis evaluation metrics (P@K, R@K, mAP@K) on test split.

    This endpoint evaluates model performance using:
    - Precision@K: Fraction of top-K results that are relevant
    - Recall@K: Fraction of all relevant items found in top-K
    - mAP@K: Mean Average Precision at K
    """
    try:
        embedder = model_manager.get_embedder(model)
        if not embedder:
            raise HTTPException(
                status_code=503, detail=f"Model '{model}' not available."
            )

        # Query test split images
        query_images = (
            db.query(ProductImage)
            .join(Product)
            .filter(
                Product.public_metadata["split"].astext == "test",
                ProductImage.type == "Search",
            )
            .order_by(func.random())
            .limit(sample_size)
            .all()
        )

        if not query_images:
            raise HTTPException(status_code=404, detail="No test split data found.")

        from collections import defaultdict
        import numpy as np

        TOP_K = [5, 10]
        global_metrics = {str(k): {"mP": [], "mR": [], "mAP": []} for k in TOP_K}
        cat_stats = defaultdict(lambda: {"p10": [], "r10": [], "ap10": []})

        prefix = get_embedding_prefix(model)
        emb_col = f"embedding_{prefix}"

        for q in query_images:
            meta = SearchService.get_categorization(db, q.product_id)
            gt_type = meta["article_type"]

            # Count total relevant items in database
            total_rel = (
                db.query(func.count(Product.id))
                .join(ProductClassification)
                .join(Taxon)
                .filter(Taxon.name == gt_type, Product.id != q.product_id)
                .scalar()
            )

            if total_rel == 0:
                continue

            vector = getattr(q, emb_col)
            if vector is None:
                continue

            # Get top-10 results
            hits = SearchService.search_by_vector(
                db, vector, model, limit=10, exclude_image_id=q.id
            )
            relevance = [h.article_type == gt_type for h in hits]

            # Calculate metrics at different K values
            for k in TOP_K:
                k_rel = relevance[:k]
                p_k = sum(k_rel) / k
                r_k = sum(k_rel) / total_rel

                # Average Precision@K
                ap_k = 0.0
                if any(k_rel):
                    precs = [
                        sum(k_rel[: i + 1]) / (i + 1) for i, r in enumerate(k_rel) if r
                    ]
                    ap_k = np.mean(precs) if precs else 0.0

                global_metrics[str(k)]["mP"].append(p_k)
                global_metrics[str(k)]["mR"].append(r_k)
                global_metrics[str(k)]["mAP"].append(ap_k)

                # Category-level metrics (at K=10 only)
                if k == 10:
                    cat_stats[gt_type]["p10"].append(p_k)
                    cat_stats[gt_type]["r10"].append(r_k)
                    cat_stats[gt_type]["ap10"].append(ap_k)

        # Aggregate metrics
        final_global = {
            k: {
                "mP": float(np.mean(v["mP"])) if v["mP"] else 0.0,
                "mR": float(np.mean(v["mR"])) if v["mR"] else 0.0,
                "mAP": float(np.mean(v["mAP"])) if v["mAP"] else 0.0,
            }
            for k, v in global_metrics.items()
        }

        final_cats = [
            CategoryMetric(
                category=c,
                mP_10=float(np.mean(v["p10"])),
                mR_10=float(np.mean(v["r10"])),
                mAP_10=float(np.mean(v["ap10"])),
                sample_size=len(v["p10"]),
            )
            for c, v in cat_stats.items()
            if v["p10"]
        ]

        return EvaluationResponse(
            model=model,
            global_metrics=final_global,
            category_breakdown=sorted(final_cats, key=lambda x: x.mAP_10, reverse=True),
        )

    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Evaluation error: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=str(e))


# Run with: uvicorn app.main:app --reload --host 0.0.0.0 --port 8000

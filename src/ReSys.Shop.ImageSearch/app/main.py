"""
This module defines the main FastAPI application for the ReSys.Shop Image Search service.

It sets up API endpoints for:
- Health checks and model listings.
- Generating image embeddings in batches.
- Performing visual search via image ID or direct upload.
- Providing product recommendations based on visual similarity.
- Diagnostic tools for comparing model performance.
"""

from fastapi import FastAPI, File, UploadFile, Depends, HTTPException, Query, Form, Header, Path
from fastapi.responses import JSONResponse
from fastapi.middleware.cors import CORSMiddleware
from fastapi.security import APIKeyHeader
from sqlalchemy.orm import Session
from sqlalchemy import text
from typing import List, Optional, Dict
from pydantic import BaseModel, Field
import io, os, time
import shutil
import uuid
from PIL import Image
from datetime import datetime

from database import get_db, ProductImage, Product
from model_factory import get_embedder

app = FastAPI(
    title="Fashion Image Search API", 
    version="3.3.0",
    description="API for visual search, recommendation, and embedding generation in a fashion catalog."
)

app.add_middleware(CORSMiddleware, allow_origins=["*"], allow_credentials=True, allow_methods=["*"], allow_headers=["*"])

# --- Constants and Configuration ---
MODEL_TYPE = os.getenv('MODEL_TYPE', 'clip')
UPLOAD_DIR = os.getenv("UPLOAD_DIR", "data/uploads")
ALLOWED_EXTENSIONS = {'png', 'jpg', 'jpeg', 'webp'}
API_KEY = os.getenv("API_KEY", "thesis-secure-api-key-2025")
API_KEY_NAME = "X-API-Key"

api_key_header = APIKeyHeader(name=API_KEY_NAME, auto_error=True)
os.makedirs(UPLOAD_DIR, exist_ok=True)
embedders = {} # Global cache for loaded models

# --- Security and Dependencies ---
async def get_api_key(api_key: str = Depends(api_key_header)):
    """Dependency to validate the API key in the request header."""
    if api_key != API_KEY:
        raise HTTPException(status_code=403, detail="Could not validate API Key")
    return api_key

def get_cached_embedder(name: str):
    """Retrieves a model from the cache or loads it if not present."""
    if name not in embedders:
        print(f"Loading model '{name}' into cache...")
        embedders[name] = get_embedder(name)
    return embedders[name]

def allowed_file(filename: str) -> bool:
    """Checks if the uploaded file has an allowed extension."""
    return '.' in filename and filename.rsplit('.', 1)[1].lower() in ALLOWED_EXTENSIONS

# --- Pydantic Models for API Schema ---
class SearchResultItem(BaseModel):
    """Represents a single item in a search result list."""
    product_id: uuid.UUID
    image_id: uuid.UUID
    image_url: str
    score: float = Field(..., example=0.9876, description="Similarity score (0.0 to 1.0, higher is better).")
    class Config:
        from_attributes = True

class SearchResponse(BaseModel):
    """Standard response model for search operations."""
    model: str = Field(..., example="clip")
    count: int = Field(..., example=10)
    results: List[SearchResultItem]

class RecommendationResponse(BaseModel):
    """Response model for product recommendation requests."""
    source_product_id: uuid.UUID
    model: str = Field(..., example="clip")
    count: int = Field(..., example=10)
    results: List[SearchResultItem]
    
class ModelInfo(BaseModel):
    """Describes the architecture and embedding dimension of a model."""
    arch: str
    dim: int

class ModelsResponse(BaseModel):
    """Response model for listing available models."""
    total_models: int
    default_model: str
    models: dict[str, ModelInfo]

class BatchGenerateRequest(BaseModel):
    """Request model for the batch embedding generation endpoint."""
    image_ids: List[uuid.UUID]

class ProcessReportItem(BaseModel):
    """Details the processing status for a single image in a batch operation."""
    image_id: uuid.UUID
    status: str
    updated_embeddings: List[str]
    error: Optional[str] = None

class BatchGenerateResponse(BaseModel):
    """Response model for the batch embedding generation endpoint."""
    successful: int
    failed: int
    details: List[ProcessReportItem]

class CompareResultItem(BaseModel):
    """Contains the search results for a single model in a comparison."""
    model: str
    count: int
    results: List[SearchResultItem]

class CompareResponse(BaseModel):
    """Response model for the model comparison diagnostic endpoint."""
    source_image_id: uuid.UUID
    results_by_model: Dict[str, CompareResultItem]


# --- API Endpoints ---
@app.on_event("startup")
async def startup_event():
    """Pre-loads the default model on application startup to reduce latency of first request."""
    get_cached_embedder(MODEL_TYPE)
    print(f"✓ API ready with default model: {MODEL_TYPE}")

@app.get("/", tags=["General"])
async def root():
    """Root endpoint providing basic service information."""
    return {"service": "Fashion Image Search", "version": "3.3.0", "default_model": MODEL_TYPE}

@app.get("/health", tags=["General"])
async def health_check(db: Session = Depends(get_db)):
    """Performs a health check on the service and its database connection."""
    try:
        db.execute(text("SELECT 1"))
        count = db.query(ProductImage).count()
        return {"status": "healthy", "database": "connected", "model": MODEL_TYPE, "indexed_images": count}
    except Exception as e:
        raise HTTPException(status_code=503, content={"status": "unhealthy", "error": str(e)})

@app.get("/models", response_model=ModelsResponse, dependencies=[Depends(get_api_key)], tags=["General"])
async def list_models():
    """Get a list of available embedding models and their specifications."""
    return {
        "total_models": 3,
        "default_model": MODEL_TYPE,
        "models": {
            "mobilenet_v3": {"arch": "Efficient CNN", "dim": 576},
            "efficientnet_b0": {"arch": "Scaled CNN", "dim": 1280},
            "clip": {"arch": "Transformer", "dim": 512}
        }
    }

# --- Embeddings Endpoints ---
@app.post("/embeddings/generate", response_model=BatchGenerateResponse, dependencies=[Depends(get_api_key)], tags=["Embeddings"])
async def generate_embeddings_batch(
    request: BatchGenerateRequest,
    db: Session = Depends(get_db)
):
    """
    Generate and save missing embeddings for a batch of images. This is the primary
    endpoint for background processing, intended to be called by a recurring job.
    """
    success_count = 0
    fail_count = 0
    report_details = []
    
    models_to_check = {
        "mobilenet_v3": "embedding_mobilenet",
        "efficientnet_b0": "embedding_efficientnet",
        "clip": "embedding_clip"
    }

    for image_id in request.image_ids:
        updated_embeddings = []
        try:
            # Query for the image to process
            image = db.query(ProductImage).filter(ProductImage.id == image_id).first()
            if not image:
                fail_count += 1
                report_details.append(ProcessReportItem(image_id=image_id, status="failed", updated_embeddings=[], error="Image not found"))
                continue

            image_changed = False
            # Check each model to see if embeddings are missing
            for model_name, emb_col in models_to_check.items():
                if getattr(image, emb_col) is None:
                    # If an embedding is missing, generate it
                    embedder = get_cached_embedder(model_name)
                    embedding = embedder.extract_features(image.url)
                    
                    # Update the image object with the new embedding and metadata
                    setattr(image, emb_col, embedding)
                    setattr(image, f"{emb_col}_model", embedder.name)
                    setattr(image, f"{emb_col}_generated_at", datetime.utcnow())
                    
                    updated_embeddings.append(model_name)
                    image_changed = True
            
            if image_changed:
                db.add(image)

            success_count += 1
            report_details.append(ProcessReportItem(image_id=image_id, status="success", updated_embeddings=updated_embeddings))

        except Exception as e:
            fail_count += 1
            report_details.append(ProcessReportItem(image_id=image_id, status="failed", updated_embeddings=updated_embeddings, error=str(e)))
    
    try:
        # Commit all changes for the batch at once
        db.commit()
    except Exception as e:
        db.rollback()
        raise HTTPException(status_code=500, detail=f"Database commit failed: {e}")

    return {"successful": success_count, "failed": fail_count, "details": report_details}

# --- Search Endpoints ---
@app.post("/search/by-upload", response_model=SearchResponse, dependencies=[Depends(get_api_key)], tags=["Search"])
async def search_by_upload(
    file: UploadFile = File(..., description="Image file to use as the search query."), 
    limit: int = Query(10, ge=1, le=100, description="Maximum number of results to return."), 
    model: str = Query(MODEL_TYPE, enum=["mobilenet_v3", "efficientnet_b0", "clip"], description="Embedding model to use for the search."),
    db: Session = Depends(get_db)
):
    """Search for similar images by uploading an image file."""
    try:
        contents = await file.read()
        image = Image.open(io.BytesIO(contents))
        embedder = get_cached_embedder(model)
        vector = embedder.extract_features(image)
        
        emb_col = {"mobilenet_v3": "embedding_mobilenet", "efficientnet_b0": "embedding_efficientnet", "clip": "embedding_clip"}[model]
        
        # SQL query uses the '<=>' (cosine distance) operator from pgvector for efficient search
        sql = text(f"""
            SELECT pi.product_id, pi.id as image_id, pi.url, 1 - (pi.{emb_col} <=> :vec) as similarity 
            FROM product_images pi 
            WHERE pi.{emb_col} IS NOT NULL 
            ORDER BY pi.{emb_col} <=> :vec 
            LIMIT :limit
        """)
        
        results = db.execute(sql, {"vec": str(vector), "limit": limit}).fetchall()
        
        return {
            "model": model,
            "count": len(results),
            "results": [{"product_id": r[0], "image_id": r[1], "image_url": r[2], "score": float(r[3])} for r in results]
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/search/by-id/{image_id}", response_model=SearchResponse, dependencies=[Depends(get_api_key)], tags=["Search"])
async def search_by_id(
    image_id: uuid.UUID = Path(..., description="UUID of the image in the database to use as the query."),
    limit: int = Query(10, ge=1, le=100, description="Maximum number of results to return."),
    model: str = Query(MODEL_TYPE, enum=["mobilenet_v3", "efficientnet_b0", "clip"], description="Embedding model to use for the search."),
    db: Session = Depends(get_db)
):
    """Search for similar images using an existing image ID from the database."""
    source_image = db.query(ProductImage).filter(ProductImage.id == image_id).first()
    if not source_image:
        raise HTTPException(status_code=404, detail="Source image not found")

    emb_col = {"mobilenet_v3": "embedding_mobilenet", "efficientnet_b0": "embedding_efficientnet", "clip": "embedding_clip"}[model]
    source_vector = getattr(source_image, emb_col)

    if source_vector is None:
        raise HTTPException(status_code=404, detail=f"Embedding for model '{model}' not found for this image. Please generate it first.")
    
    # Use pgvector's <=> operator to find the nearest neighbors in the vector space.
    # `1 - (distance)` converts cosine distance to cosine similarity.
    sql = text(f"""
        SELECT pi.product_id, pi.id as image_id, pi.url, 1 - (pi.{emb_col} <=> :vec) as similarity 
        FROM product_images pi 
        WHERE pi.id != :src_id AND pi.{emb_col} IS NOT NULL 
        ORDER BY pi.{emb_col} <=> :vec 
        LIMIT :limit
    """)
    
    results = db.execute(sql, {"vec": str(source_vector.tolist()), "limit": limit, "src_id": image_id}).fetchall()
    
    return {
        "model": model,
        "count": len(results),
        "results": [{"product_id": r[0], "image_id": r[1], "image_url": r[2], "score": float(r[3])} for r in results]
    }

# --- Recommendations Endpoint ---
@app.get("/recommendations/by-product-id/{product_id}", response_model=RecommendationResponse, dependencies=[Depends(get_api_key)], tags=["Recommendations"])
async def recommendations_by_product(
    product_id: uuid.UUID = Path(..., description="UUID of the product to get recommendations for."),
    limit: int = Query(10, ge=1, le=100, description="Maximum number of recommendations to return."),
    model: str = Query(MODEL_TYPE, enum=["mobilenet_v3", "efficientnet_b0", "clip"], description="Embedding model to use for recommendations."),
    db: Session = Depends(get_db)
):
    """Recommend similar products based on the visual features of a given product."""
    source_product = db.query(Product).filter(Product.id == product_id).first()
    if not source_product:
        raise HTTPException(status_code=404, detail="Source product not found")
        
    # Find the primary 'Search' image for the product. Fallback to any image if not found.
    source_image = db.query(ProductImage).filter(
        ProductImage.product_id == product_id,
        ProductImage.type == 'Search'
    ).order_by(ProductImage.position).first()
    
    if not source_image:
        source_image = db.query(ProductImage).filter(
            ProductImage.product_id == product_id
        ).order_by(ProductImage.position).first()

    if not source_image:
        raise HTTPException(status_code=404, detail="No images found for the source product to generate recommendations.")

    # Use the search function to find similar images, fetching a few extra to filter out the source product
    search_response_dict = await search_by_id(source_image.id, limit + 5, model, db)
    search_response = SearchResponse.model_validate(search_response_dict)

    # Filter out other images belonging to the same source product
    filtered_results = []
    for res in search_response.results:
        if res.product_id != product_id:
            filtered_results.append(res)
        if len(filtered_results) >= limit:
            break
            
    return {
        "source_product_id": product_id,
        "model": model,
        "count": len(filtered_results),
        "results": filtered_results
    }

# --- Diagnostics Endpoint ---
@app.get("/diagnostics/compare-models/{image_id}", response_model=CompareResponse, dependencies=[Depends(get_api_key)], tags=["Diagnostics"])
async def compare_models(
    image_id: uuid.UUID = Path(..., description="UUID of the image to use as the query for comparison."),
    limit: int = Query(5, ge=1, le=50, description="Number of results to return per model."),
    models: List[str] = Query(default=["mobilenet_v3", "efficientnet_b0", "clip"], description="Models to compare."),
    db: Session = Depends(get_db)
):
    """
    Run a search for the same image across multiple models to compare their results.
    This is a diagnostic tool to evaluate model performance.
    """
    source_image = db.query(ProductImage).filter(ProductImage.id == image_id).first()
    if not source_image:
        raise HTTPException(status_code=404, detail="Source image not found")

    comparison_results = {}
    valid_models = {"mobilenet_v3": "embedding_mobilenet", "efficientnet_b0": "embedding_efficientnet", "clip": "embedding_clip"}

    for model_name in models:
        if model_name not in valid_models:
            continue

        emb_col = valid_models[model_name]
        source_vector = getattr(source_image, emb_col)
        
        if source_vector is None:
            comparison_results[model_name] = {"model": model_name, "count": 0, "results": []}
            continue

        sql = text(f"""
            SELECT pi.product_id, pi.id as image_id, pi.url, 1 - (pi.{emb_col} <=> :vec) as similarity 
            FROM product_images pi 
            WHERE pi.id != :src_id AND pi.{emb_col} IS NOT NULL 
            ORDER BY pi.{emb_col} <=> :vec 
            LIMIT :limit
        """)
        
        results_cursor = db.execute(sql, {"vec": str(source_vector.tolist()), "limit": limit, "src_id": image_id}).fetchall()
        
        search_results = [{"product_id": r[0], "image_id": r[1], "image_url": r[2], "score": float(r[3])} for r in results_cursor]
        
        comparison_results[model_name] = {
            "model": model_name,
            "count": len(search_results),
            "results": search_results
        }
    
    return {"source_image_id": image_id, "results_by_model": comparison_results}

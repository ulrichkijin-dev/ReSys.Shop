"""
This module defines the main FastAPI application for the ReSys.Shop Image Search service.

Exposes endpoints for:
- Visual search and recommendations.
- Batch embedding generation.
- Evaluation metrics (P@K, R@K, mAP@K) for thesis analysis.
"""

from fastapi import FastAPI, File, UploadFile, Depends, HTTPException, Query, Form, Header, Path
from fastapi.responses import JSONResponse
from fastapi.middleware.cors import CORSMiddleware
from fastapi.security import APIKeyHeader
from sqlalchemy.orm import Session
from sqlalchemy import text, func
from typing import List, Optional, Dict, Any
from pydantic import BaseModel, Field
import io, os, time
import shutil
import uuid
import logging
import numpy as np
from PIL import Image
from datetime import datetime
from collections import defaultdict

logger = logging.getLogger(__name__)

from app.database import get_db, ProductImage, Product, Taxon, ProductClassification
from app.model_factory import get_embedder

app = FastAPI(
    title="Fashion Image Search API", 
    version="3.5.0",
    description="API for visual search, recommendation, and thesis evaluation metrics."
)

app.add_middleware(CORSMiddleware, allow_origins=["*"], allow_credentials=True, allow_methods=["*"], allow_headers=["*"])

# --- Configuration ---
MODEL_TYPE = os.getenv('MODEL_TYPE', 'efficientnet_b0')
UPLOAD_DIR = os.getenv("UPLOAD_DIR", "data/uploads")
API_KEY = os.getenv("API_KEY", "thesis-secure-api-key-2025")
API_KEY_NAME = "X-API-Key"

api_key_header = APIKeyHeader(name=API_KEY_NAME, auto_error=True)
os.makedirs(UPLOAD_DIR, exist_ok=True)
embedders = {}

# --- Helper Functions ---
async def get_api_key(api_key: str = Depends(api_key_header)):
    if api_key != API_KEY:
        raise HTTPException(status_code=403, detail="Could not validate API Key")
    return api_key

def get_cached_embedder(name: str):
    if name not in embedders:
        logger.info("Loading model '%s' into cache...", name)
        model = get_embedder(name)
        if model is None and name != "clip":
            raise HTTPException(status_code=500, detail=f"Model '{name}' could not be loaded.")
        embedders[name] = model
    return embedders[name]

def get_categorization(db: Session, product_id: uuid.UUID) -> dict:
    """Helper to fetch category metadata for a product."""
    res = db.query(Taxon.name, Taxon.parent_id).join(
        ProductClassification, ProductClassification.taxon_id == Taxon.id
    ).filter(ProductClassification.product_id == product_id).first()
    
    if not res:
        return {"article_type": "Unknown", "sub_category": "Unknown"}
    
    article_type = res[0]
    sub_cat_id = res[1]
    sub_cat = "Unknown"
    
    if sub_cat_id:
        sc = db.query(Taxon.name).filter(Taxon.id == sub_cat_id).first()
        if sc: sub_cat = sc[0]
        
    return {"article_type": article_type, "sub_category": sub_cat}

# --- Pydantic Models ---
class SearchResultItem(BaseModel):
    product_id: uuid.UUID
    image_id: uuid.UUID
    image_url: str
    score: float
    article_type: Optional[str] = None
    sub_category: Optional[str] = None

class SearchResponse(BaseModel):
    model: str
    count: int
    results: List[SearchResultItem]

class RecommendationResponse(BaseModel):
    source_product_id: uuid.UUID
    model: str
    count: int
    results: List[SearchResultItem]

class CategoryMetric(BaseModel):
    category: str
    mP_10: float
    mR_10: float
    mAP_10: float
    sample_size: int

class EvaluationResponse(BaseModel):
    model: str
    global_metrics: Dict[str, Dict[str, float]]
    article_type_breakdown: List[CategoryMetric]
    sub_category_breakdown: List[CategoryMetric]

# --- API Endpoints ---

@app.on_event("startup")
async def startup_event():
    get_cached_embedder(MODEL_TYPE)
    get_cached_embedder("clip")
    logger.info("API ready with default model: %s", MODEL_TYPE)

@app.get("/", tags=["General"])
async def root():
    return {"service": "Fashion Image Search", "version": "3.5.0", "active_model": MODEL_TYPE}

@app.get("/health", tags=["General"])
async def health_check(db: Session = Depends(get_db)):
    try:
        db.execute(text("SELECT 1"))
        count = db.query(ProductImage).count()
        models_info = []
        for m_name in ["mobilenet_v3", "efficientnet_b0", "clip"]:
            emb = embedders.get(m_name)
            models_info.append({
                "model_name": m_name,
                "loaded": emb is not None,
                "dimensions": getattr(emb, 'dim', None) if emb else None
            })
        return {"status": "healthy", "database": "connected", "indexed_images": count, "models": models_info}
    except Exception as e:
        raise HTTPException(status_code=503, detail=str(e))

@app.get("/models", tags=["General"])
async def list_models():
    models_info = {}
    for m_name in ["mobilenet_v3", "efficientnet_b0", "clip"]:
        emb = embedders.get(m_name)
        models_info[m_name] = {
            "model_name": m_name,
            "loaded": emb is not None,
            "dimensions": getattr(emb, 'dim', None) if emb else None
        }
    return {"total_models": 3, "default_model": MODEL_TYPE, "models": models_info}

@app.post("/search/by-upload", response_model=SearchResponse, dependencies=[Depends(get_api_key)], tags=["Search"])
async def search_by_upload(
    file: UploadFile = File(...), 
    limit: int = Query(10, ge=1, le=100), 
    model: str = Query(MODEL_TYPE, enum=["mobilenet_v3", "efficientnet_b0", "clip"]),
    db: Session = Depends(get_db)
):
    try:
        contents = await file.read()
        image = Image.open(io.BytesIO(contents))
        embedder = get_cached_embedder(model)
        vector = embedder.extract_features(image)
        vec = vector.tolist() if hasattr(vector, 'tolist') else vector
        emb_col = {"mobilenet_v3": "embedding_mobilenet", "efficientnet_b0": "embedding_efficientnet", "clip": "embedding_clip"}[model]
        sql = text(f"SELECT pi.product_id, pi.id, pi.url, 1 - (pi.{emb_col} <=> CAST(:vec AS vector)) FROM product_images pi WHERE pi.{emb_col} IS NOT NULL ORDER BY pi.{emb_col} <=> CAST(:vec AS vector) LIMIT :limit")
        results = db.execute(sql, {"vec": vec, "limit": limit}).fetchall()
        items = []
        for r in results:
            meta = get_categorization(db, r[0])
            items.append(SearchResultItem(product_id=r[0], image_id=r[1], image_url=r[2], score=float(r[3]), article_type=meta["article_type"], sub_category=meta["sub_category"]))
        return {"model": model, "count": len(items), "results": items}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/search/by-id/{image_id}", response_model=SearchResponse, dependencies=[Depends(get_api_key)], tags=["Search"])
async def search_by_id(
    image_id: uuid.UUID,
    limit: int = Query(10, ge=1, le=100),
    model: str = Query(MODEL_TYPE, enum=["mobilenet_v3", "efficientnet_b0", "clip"]),
    db: Session = Depends(get_db)
):
    source_image = db.query(ProductImage).filter(ProductImage.id == image_id).first()
    if not source_image: raise HTTPException(status_code=404, detail="Source image not found")
    emb_col = {"mobilenet_v3": "embedding_mobilenet", "efficientnet_b0": "embedding_efficientnet", "clip": "embedding_clip"}[model]
    vector = getattr(source_image, emb_col)
    if vector is None: raise HTTPException(status_code=400, detail=f"Embedding for {model} missing")
    vec = vector.tolist() if hasattr(vector, 'tolist') else vector
    sql = text(f"SELECT pi.product_id, pi.id, pi.url, 1 - (pi.{emb_col} <=> CAST(:vec AS vector)) FROM product_images pi WHERE pi.id != :src_id AND pi.{emb_col} IS NOT NULL ORDER BY pi.{emb_col} <=> CAST(:vec AS vector) LIMIT :limit")
    results = db.execute(sql, {"vec": vec, "src_id": image_id, "limit": limit}).fetchall()
    items = []
    for r in results:
        meta = get_categorization(db, r[0])
        items.append(SearchResultItem(product_id=r[0], image_id=r[1], image_url=r[2], score=float(r[3]), article_type=meta["article_type"], sub_category=meta["sub_category"]))
    return {"model": model, "count": len(items), "results": items}

@app.get("/recommendations/by-product-id/{product_id}", response_model=RecommendationResponse, dependencies=[Depends(get_api_key)], tags=["Recommendations"])
async def recommendations_by_product(
    product_id: uuid.UUID,
    limit: int = Query(10, ge=1, le=100),
    model: str = Query(MODEL_TYPE, enum=["mobilenet_v3", "efficientnet_b0", "clip"]),
    db: Session = Depends(get_db)
):
    source_image = db.query(ProductImage).filter(ProductImage.product_id == product_id, ProductImage.type == 'Search').first()
    if not source_image: source_image = db.query(ProductImage).filter(ProductImage.product_id == product_id).first()
    if not source_image: raise HTTPException(status_code=404, detail="No image found")
    emb_col = {"mobilenet_v3": "embedding_mobilenet", "efficientnet_b0": "embedding_efficientnet", "clip": "embedding_clip"}[model]
    vector = getattr(source_image, emb_col)
    if vector is None: raise HTTPException(status_code=400, detail=f"Embedding missing")
    vec = vector.tolist() if hasattr(vector, 'tolist') else vector
    sql = text(f"SELECT pi.product_id, pi.id, pi.url, 1 - (pi.{emb_col} <=> CAST(:vec AS vector)) FROM product_images pi WHERE pi.product_id != :src_id AND pi.{emb_col} IS NOT NULL ORDER BY pi.{emb_col} <=> CAST(:vec AS vector) LIMIT :limit")
    results = db.execute(sql, {"vec": vec, "src_id": product_id, "limit": limit}).fetchall()
    items = []
    for r in results:
        meta = get_categorization(db, r[0])
        items.append(SearchResultItem(product_id=r[0], image_id=r[1], image_url=r[2], score=float(r[3]), article_type=meta["article_type"], sub_category=meta["sub_category"]))
    return {"source_product_id": product_id, "model": model, "count": len(items), "results": items}

@app.get("/evaluation/metrics", response_model=EvaluationResponse, dependencies=[Depends(get_api_key)], tags=["Thesis"])
async def get_evaluation_metrics(
    model: str = Query("efficientnet_b0", enum=["mobilenet_v3", "efficientnet_b0", "clip"]),
    sample_size: int = Query(20, ge=1, le=100),
    db: Session = Depends(get_db)
):
    query_images = db.query(ProductImage).join(Product).filter(Product.public_metadata['split'].astext == 'test', ProductImage.type == 'Search').order_by(func.random()).limit(sample_size).all()
    if not query_images: raise HTTPException(status_code=404, detail="No test data")
    
    TOP_K = [5, 10, 20]
    global_metrics = {str(k): {"mP": [], "mR": [], "mAP": []} for k in TOP_K}
    art_stats = defaultdict(lambda: {"p10": [], "r10": [], "ap10": []})
    sub_stats = defaultdict(lambda: {"p10": [], "r10": [], "ap10": []})
    emb_col = {"mobilenet_v3": "embedding_mobilenet", "efficientnet_b0": "embedding_efficientnet", "clip": "embedding_clip"}[model]

    for q in query_images:
        meta = get_categorization(db, q.product_id)
        gt_art, gt_sub = meta["article_type"], meta["sub_category"]
        total_rel = db.query(func.count(Product.id)).join(ProductClassification).join(Taxon).filter(Taxon.name == gt_art, Product.id != q.product_id).scalar()
        if total_rel == 0: continue
        vector = getattr(q, emb_col)
        if vector is None: continue
        vec = vector.tolist() if hasattr(vector, 'tolist') else vector
        sql = text(f"SELECT pi.product_id FROM product_images pi WHERE pi.id != :src_id AND pi.{emb_col} IS NOT NULL ORDER BY pi.{emb_col} <=> CAST(:vec AS vector) LIMIT 20")
        hits = db.execute(sql, {"vec": vec, "src_id": q.id}).fetchall()
        relevance = [get_categorization(db, h[0])["article_type"] == gt_art for h in hits]

        for k in TOP_K:
            k_rel = relevance[:k]
            p_k, r_k = sum(k_rel) / k, sum(k_rel) / total_rel
            ap_k = np.mean([sum(k_rel[:i+1])/(i+1) for i, r in enumerate(k_rel) if r]) if any(k_rel) else 0.0
            global_metrics[str(k)]["mP"].append(p_k)
            global_metrics[str(k)]["mR"].append(r_k)
            global_metrics[str(k)]["mAP"].append(ap_k)
            if k == 10:
                for stat_dict, key in [(art_stats, gt_art), (sub_stats, gt_sub)]:
                    stat_dict[key]["p10"].append(p_k)
                    stat_dict[key]["r10"].append(r_k)
                    stat_dict[key]["ap10"].append(ap_k)

    def aggregate(stats):
        return sorted([CategoryMetric(category=c, mP_10=float(np.mean(v["p10"])), mR_10=float(np.mean(v["r10"])), mAP_10=float(np.mean(v["ap10"])), sample_size=len(v["p10"])) for c, v in stats.items()], key=lambda x: x.mAP_10, reverse=True)

    return {
        "model": model,
        "global_metrics": {k: {"mP": float(np.mean(v["mP"])) if v["mP"] else 0, "mR": float(np.mean(v["mR"])) if v["mR"] else 0, "mAP": float(np.mean(v["mAP"])) if v["mAP"] else 0} for k, v in global_metrics.items()},
        "article_type_breakdown": aggregate(art_stats),
        "sub_category_breakdown": aggregate(sub_stats)
    }
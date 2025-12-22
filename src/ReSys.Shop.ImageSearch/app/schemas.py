from pydantic import BaseModel, Field
from typing import List, Optional, Dict
import uuid
from datetime import datetime

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
    category_breakdown: List[CategoryMetric]

class ModelStatus(BaseModel):
    model_name: str
    architecture: str
    loaded: bool
    dimensions: Optional[int] = None
    error: Optional[str] = None

class HealthResponse(BaseModel):
    status: str
    database: str
    indexed_images: int
    models: List[ModelStatus]

class ModelsListResponse(BaseModel):
    total_models: int
    default_model: str
    models: Dict[str, ModelStatus]

class EmbeddingGenerationRequest(BaseModel):
    image_ids: List[str]

class EmbeddingDetail(BaseModel):
    image_id: str
    status: str
    updated_embeddings: List[str]
    error: Optional[str] = None

class EmbeddingGenerationResponse(BaseModel):
    successful: int
    failed: int
    details: List[EmbeddingDetail]

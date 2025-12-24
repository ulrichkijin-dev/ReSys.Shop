import uuid
import logging
from typing import List, Optional, Dict, Any
from sqlalchemy.orm import Session
from sqlalchemy import text
import numpy as np

from app.database import ProductImage, Taxon, ProductClassification
from app.model_factory import model_manager
from app.schemas import SearchResultItem

logger = logging.getLogger(__name__)

class SearchService:
    @staticmethod
    def get_categorization(db: Session, product_id: uuid.UUID) -> dict:
        """Fetch category metadata for a product."""
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
            if sc: 
                sub_cat = sc[0]
            
        return {"article_type": article_type, "sub_category": sub_cat}

    @staticmethod
    def search_by_vector(
        db: Session, 
        vector: List[float], 
        model_name: str, 
        limit: int = 10,
        exclude_image_id: Optional[uuid.UUID] = None,
        exclude_product_id: Optional[uuid.UUID] = None
    ) -> List[SearchResultItem]:
        """Search database using cosine distance for a given vector."""
        emb_col = {
            "resnet50": "embedding_resnet",
            "mobilenet_v3": "embedding_mobilenet", 
            "efficientnet_b0": "embedding_efficientnet", 
            "convnext_tiny": "embedding_convnext",
            "clip_vit_b16": "embedding_clip",
            "fashion_clip": "embedding_fclip",
            "dinov2_vits14": "embedding_dino"
        }.get(model_name, "embedding_efficientnet")

        # Ensure vector is a list for psycopg2/pgvector
        if hasattr(vector, 'tolist'):
            vector = vector.tolist()
        elif isinstance(vector, np.ndarray):
            vector = vector.tolist()
            
        where_clauses = [f"pi.{emb_col} IS NOT NULL"]
        params = {"vec": vector, "limit": limit}

        if exclude_image_id:
            where_clauses.append("pi.id != :exclude_image_id")
            params["exclude_image_id"] = exclude_image_id
            
        if exclude_product_id:
            where_clauses.append("pi.product_id != :exclude_product_id")
            params["exclude_product_id"] = exclude_product_id

        where_stmt = " AND ".join(where_clauses)
        
        sql = text(f"""
            SELECT pi.product_id, pi.id as image_id, pi.url, 1 - (pi.{emb_col} <=> CAST(:vec AS vector)) as similarity 
            FROM eshopdb.product_images pi 
            WHERE {where_stmt}
            ORDER BY pi.{emb_col} <=> CAST(:vec AS vector) 
            LIMIT :limit
        """)
        
        results = db.execute(sql, params).fetchall()
        
        items = []
        for r in results:
            meta = SearchService.get_categorization(db, r[0])
            items.append(SearchResultItem(
                product_id=r[0], 
                image_id=r[1], 
                image_url=r[2], 
                score=float(r[3]),
                article_type=meta["article_type"], 
                sub_category=meta["sub_category"]
            ))
        return items

    @staticmethod
    def get_model_info() -> Dict[str, Dict[str, Any]]:
        """Get status of all supported models."""
        model_info = {
            "efficientnet_b0": "Production Baseline CNN (EfficientNet-B0)",
            "convnext_tiny": "Modern CNN (ConvNeXt-Tiny)",
            "clip_vit_b16": "General Semantic Transformer (CLIP ViT-B/16)",
            "fashion_clip": "Domain-Specific Transformer (Fashion-CLIP)",
            "dinov2_vits14": "Visual Structure Transformer (DINOv2 ViT-S/14)"
        }
        
        loaded_models = model_manager.get_loaded_models()
        
        result = {}
        for name, arch in model_info.items():
            embedder = loaded_models.get(name)
            if embedder:
                result[name] = {
                    "architecture": arch,
                    "loaded": True,
                    "dimensions": embedder.dim
                }
            else:
                result[name] = {
                    "architecture": arch,
                    "loaded": False,
                    "dimensions": None
                }
        return result

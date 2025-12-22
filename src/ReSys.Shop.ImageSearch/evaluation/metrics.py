import os
import uuid
from collections import defaultdict
from sqlalchemy.orm import Session
from sqlalchemy import func
import numpy as np
import pandas as pd
import sys
from pathlib import Path

# Add parent directory to path to import app
sys.path.append(str(Path(__file__).resolve().parent.parent))

from app.config import settings
from app.database import get_db, Product, ProductImage, ProductClassification, Taxon
from app.services.search_service import SearchService
from app.model_factory import model_manager

# Evaluation parameters
SAMPLE_SIZE = 50
TOP_K_VALUES = [5, 10, 20]
MODELS = ["efficientnet_b0", "mobilenet_v3", "clip_vit_b16", "dino_vit_s16"]

def calculate_ap(relevance_mask):
    """Calculates Average Precision."""
    if not any(relevance_mask): return 0.0
    precisions = []
    rel_count = 0
    for i, rel in enumerate(relevance_mask):
        if rel:
            rel_count += 1
            precisions.append(rel_count / (i + 1))
    return np.mean(precisions)

def run_evaluation():
    db = next(get_db())
    queries = db.query(ProductImage).join(Product).filter(
        Product.public_metadata['split'].astext == 'test',
        ProductImage.type == 'Search'
    ).order_by(func.random()).limit(SAMPLE_SIZE).all()

    if not queries:
        print("No test data found in DB.")
        return

    all_results = []
    
    # Pre-load models
    model_manager.warmup(MODELS)

    for model_name in MODELS:
        print(f"\nEvaluating {model_name}...")
        
        art_stats = defaultdict(lambda: {"p10": [], "r10": [], "ap10": []})
        model_metrics = {k: {"p": [], "r": [], "map": []} for k in TOP_K_VALUES}

        for i, q in enumerate(queries):
            meta = SearchService.get_categorization(db, q.product_id)
            gt_art = meta["article_type"]
            
            emb_col = f"embedding_{model_name.split('_')[0]}"
            vector = getattr(q, emb_col)
            if vector is None: continue

            # Search using service (internal call)
            hits = SearchService.search_by_vector(db, vector, model_name, limit=20, exclude_image_id=q.id)
            rel_mask = [h.article_type == gt_art for h in hits]

            # Total relevant in DB for Recall
            total_rel = db.query(func.count(Product.id)).join(ProductClassification).join(Taxon).filter(
                Taxon.name == gt_art, Product.id != q.product_id
            ).scalar()
            
            if total_rel == 0: continue

            for k in TOP_K_VALUES:
                k_mask = rel_mask[:k]
                p_k, r_k = sum(k_mask) / k, sum(k_mask) / total_rel
                ap_k = calculate_ap(k_mask)
                
                model_metrics[k]["p"].append(p_k)
                model_metrics[k]["r"].append(r_k)
                model_metrics[k]["map"].append(ap_k)
                
                if k == 10:
                    art_stats[gt_art]["p10"].append(p_k)
                    art_stats[gt_art]["r10"].append(r_k)
                    art_stats[gt_art]["ap10"].append(ap_k)

        for k in TOP_K_VALUES:
            all_results.append({
                "Model": model_name, "K": k,
                "mP": np.mean(model_metrics[k]["p"]) if model_metrics[k]["p"] else 0,
                "mR": np.mean(model_metrics[k]["r"]) if model_metrics[k]["r"] else 0,
                "mAP": np.mean(model_metrics[k]["map"]) if model_metrics[k]["map"] else 0
            })

        print(f"\n--- Category Breakdown for {model_name} @K=10 ---")
        df = pd.DataFrame([
            {"Name": k, "mP@10": np.mean(v["p10"]), "mR@10": np.mean(v["r10"]), "mAP@10": np.mean(v["ap10"]), "N": len(v["p10"])}
            for k, v in art_stats.items()
        ]).sort_values("mAP@10", ascending=False).head(10)
        print(df.to_string(index=False))

    print("\n" + "="*80)
    print("FINAL THESIS COMPARATIVE ANALYSIS")
    print("="*80)
    print(pd.DataFrame(all_results).to_string(index=False))

if __name__ == "__main__":
    run_evaluation()
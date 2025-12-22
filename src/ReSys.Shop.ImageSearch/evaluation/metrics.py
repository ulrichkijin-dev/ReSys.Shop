import os
import uuid
import requests
from collections import defaultdict
from sqlalchemy.orm import Session
from sqlalchemy import func
import numpy as np
import pandas as pd

# Add parent directory to path to import app
import sys
from pathlib import Path
sys.path.append(str(Path(__file__).resolve().parent.parent))

from app.database import (
    get_db,
    Product,
    ProductImage,
    Taxon,
    ProductClassification
)

# --- Configuration ---
API_BASE_URL = "http://127.0.0.1:8000"
API_KEY = os.getenv("API_KEY", "thesis-secure-api-key-2025")
HEADERS = {"X-API-Key": API_KEY}

# Evaluation parameters
SAMPLE_SIZE = 50
TOP_K_VALUES = [5, 10, 20]
MODELS = ["efficientnet_b0", "mobilenet_v3", "clip"]

def get_categorization(db: Session, product_id: uuid.UUID) -> dict:
    """Gets articleType and subCategory for a product."""
    res = db.query(Taxon.name, Taxon.parent_id).join(
        ProductClassification, ProductClassification.taxon_id == Taxon.id
    ).filter(ProductClassification.product_id == product_id).first()
    
    if not res:
        return {"articleType": "Unknown", "subCategory": "Unknown"}
    
    article_type = res[0]
    sub_cat_id = res[1]
    sub_cat = "Unknown"
    
    if sub_cat_id:
        sc = db.query(Taxon.name).filter(Taxon.id == sub_cat_id).first()
        if sc: sub_cat = sc[0]
        
    return {"articleType": article_type, "subCategory": sub_cat}

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

    for model_name in MODELS:
        print(f"\nEvaluating {model_name}...")
        
        # Breakdown trackers
        art_stats = defaultdict(lambda: {"p10": [], "r10": [], "ap10": []})
        sub_stats = defaultdict(lambda: {"p10": [], "r10": [], "ap10": []})
        model_metrics = {k: {"p": [], "r": [], "map": []} for k in TOP_K_VALUES}

        for i, q in enumerate(queries):
            meta = get_categorization(db, q.product_id)
            gt_art, gt_sub = meta["articleType"], meta["subCategory"]
            
            # Total relevant in DB for Recall
            total_rel = db.query(func.count(Product.id)).join(ProductClassification).join(Taxon).filter(
                Taxon.name == gt_art, Product.id != q.product_id
            ).scalar()
            
            if total_rel == 0: continue

            try:
                resp = requests.get(f"{API_BASE_URL}/search/by-id/{q.id}", 
                                    headers=HEADERS, params={"model": model_name, "limit": 20})
                if resp.status_code != 200: continue
                hits = resp.json().get("results", [])
            except: continue

            rel_mask = [get_categorization(db, h["product_id"])["articleType"] == gt_art for h in hits]

            for k in TOP_K_VALUES:
                k_mask = rel_mask[:k]
                p_k, r_k = sum(k_mask) / k, sum(k_mask) / total_rel
                ap_k = calculate_ap(k_mask)
                
                model_metrics[k]["p"].append(p_k)
                model_metrics[k]["r"].append(r_k)
                model_metrics[k]["map"].append(ap_k)
                
                if k == 10:
                    for s_dict, key in [(art_stats, gt_art), (sub_stats, gt_sub)]:
                        s_dict[key]["p10"].append(p_k)
                        s_dict[key]["r10"].append(r_k)
                        s_dict[key]["ap10"].append(p_k)

        # Global summary for this model
        for k in TOP_K_VALUES:
            all_results.append({
                "Model": model_name, "K": k,
                "mP": np.mean(model_metrics[k]["p"]) if model_metrics[k]["p"] else 0,
                "mR": np.mean(model_metrics[k]["r"]) if model_metrics[k]["r"] else 0,
                "mAP": np.mean(model_metrics[k]["map"]) if model_metrics[k]["map"] else 0
            })

        # Print detailed breakdowns
        for title, stats in [("ArticleType", art_stats), ("SubCategory", sub_stats)]:
            if stats:
                print(f"\n--- {title} Breakdown for {model_name} @K=10 ---")
                df = pd.DataFrame([
                    {"Name": k, "mP@10": np.mean(v["p10"]), "mR@10": np.mean(v["r10"]), "mAP@10": np.mean(v["ap10"]), "N": len(v["p10"])}
                    for k, v in stats.items()
                ]).sort_values("mAP@10", ascending=False).head(10)
                print(df.to_string(index=False))

    print("\n" + "="*80)
    print("FINAL THESIS COMPARATIVE ANALYSIS")
    print("="*80)
    print(pd.DataFrame(all_results).to_string(index=False))

if __name__ == "__main__":
    run_evaluation()
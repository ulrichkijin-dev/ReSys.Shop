import os
import uuid
import requests
from collections import defaultdict
from sqlalchemy.orm import Session
from sqlalchemy import func

# Add app directory to path to import database
import sys
from pathlib import Path
sys.path.append(str(Path(__file__).parent.parent / "app"))

from database import (
    get_db,
    Product,
    ProductImage,
    Taxon,
    ProductClassification
)

# --- Configuration ---
API_BASE_URL = "http://127.0.0.1:8000" # Assuming the FastAPI app is running here
API_KEY = os.getenv("API_KEY", "thesis-secure-api-key-2025")
API_KEY_NAME = "X-API-Key"
HEADERS = {API_KEY_NAME: API_KEY}

# Evaluation parameters
SAMPLE_SIZE = 20  # Number of query images to evaluate
TOP_K_VALUES = [5, 10, 20]
MODEL = "clip" # Model to evaluate

def get_article_type_for_product(db: Session, product_id: uuid.UUID) -> str | None:
    """Helper to get the 'articleType' taxon for a given product."""
    classification = db.query(ProductClassification).filter(ProductClassification.product_id == product_id).first()
    if not classification:
        return None
    
    # The hierarchy is master -> sub -> article. We assume articleType is the direct classification.
    # A more robust solution would traverse up the tree if needed.
    return classification.taxon.name if classification.taxon else None

def evaluate_retrieval_metrics():
    """
    Evaluates the performance of the image search API by calculating
    Precision@K and Recall@K for a sample of test images.
    """
    db = next(get_db())
    try:
        print("--- Starting Image Retrieval Evaluation ---")
        print(f"Model: {MODEL} | Sample Size: {SAMPLE_SIZE} | K values: {TOP_K_VALUES}")

        # 1. Get a sample of query images from the 'test' split
        query_images = db.query(ProductImage).join(Product).filter(
            Product.public_metadata['split'].astext == 'test',
            ProductImage.type == 'Search'
        ).order_by(func.random()).limit(SAMPLE_SIZE).all()

        if not query_images:
            print("Error: No test images found in the database. Please run the dataset loader.")
            return

        # Dictionaries to store metric sums
        avg_precision = defaultdict(float)
        avg_recall = defaultdict(float)

        # 2. For each query image, perform search and calculate metrics
        for i, query_image in enumerate(query_images):
            print(f"\n[{i+1}/{SAMPLE_SIZE}] Querying with Image ID: {query_image.id} (Product: {query_image.product_id})")

            # Get ground truth for the query image
            ground_truth_article_type = get_article_type_for_product(db, query_image.product_id)
            if not ground_truth_article_type:
                print(f"  - Warning: Could not determine ground truth article type for product {query_image.product_id}. Skipping.")
                continue
            
            print(f"  - Ground Truth Article Type: '{ground_truth_article_type}'")

            # Get total number of relevant items in the DB for recall calculation
            total_relevant_items = db.query(func.count(Product.id)).join(ProductClassification).join(Taxon).filter(
                Taxon.name == ground_truth_article_type,
                Product.id != query_image.product_id # Exclude the query product itself
            ).scalar()

            if total_relevant_items == 0:
                print("  - Warning: No other relevant items found in the database for this category. Skipping.")
                continue

            # 3. Call the search API
            try:
                max_k = max(TOP_K_VALUES)
                response = requests.get(
                    f"{API_BASE_URL}/search/by-id/{query_image.id}",
                    headers=HEADERS,
                    params={"model": MODEL, "limit": max_k}
                )
                response.raise_for_status()
                results = response.json().get("results", [])
            except requests.exceptions.RequestException as e:
                print(f"  - Error: API call failed: {e}")
                print("  - Is the FastAPI server running? `uvicorn main:app --reload --app-dir src/ReSys.Shop.ImageSearch/app`")
                return

            # 4. Calculate metrics for each K
            for k in TOP_K_VALUES:
                top_k_results = results[:k]
                if not top_k_results:
                    continue
                
                relevant_found = 0
                for item in top_k_results:
                    retrieved_product_id = item["product_id"]
                    retrieved_article_type = get_article_type_for_product(db, retrieved_product_id)
                    if retrieved_article_type == ground_truth_article_type:
                        relevant_found += 1
                
                # Precision@K = (Number of relevant items in top K) / K
                precision_at_k = relevant_found / k
                avg_precision[k] += precision_at_k

                # Recall@K = (Number of relevant items in top K) / (Total number of relevant items)
                recall_at_k = relevant_found / total_relevant_items
                avg_recall[k] += recall_at_k
                
                print(f"  - @K={k}: Precision={precision_at_k:.2f}, Recall={recall_at_k:.2f} ({relevant_found}/{k} relevant)")

        # 5. Average the metrics and print the final report
        print("\n--- Final Evaluation Report ---")
        print(f"Averaged over {len(query_images)} queries for model '{MODEL}':")
        for k in TOP_K_VALUES:
            mean_avg_precision = avg_precision[k] / len(query_images)
            mean_avg_recall = avg_recall[k] / len(query_images)
            print(f"  - P@{k}: {mean_avg_precision:.4f}")
            print(f"  - R@{k}: {mean_avg_recall:.4f}")

finally:
    db.close()

if __name__ == "__main__":
    print("Running evaluation...")
    print("Please ensure you have installed the required packages: `pip install requests sqlalchemy psycopg2-binary`")
    print("And that the API server is running.")
    evaluate_retrieval_metrics()

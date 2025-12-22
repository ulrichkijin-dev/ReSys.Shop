import requests
import os
import uuid
import sys
from pathlib import Path

# Configuration
API_BASE_URL = "http://127.0.0.1:8000"
API_KEY = "thesis-secure-api-key-2025"
HEADERS = {"X-API-Key": API_KEY}

def test_health():
    print("\n--- Testing Health Check ---")
    try:
        response = requests.get(f"{API_BASE_URL}/health")
        print(f"Status: {response.status_code}")
        print(f"Response: {response.json()}")
        return response.status_code == 200
    except Exception as e:
        print(f"Error: {e}")
        return False

def test_list_models():
    print("\n--- Testing List Models ---")
    try:
        response = requests.get(f"{API_BASE_URL}/models", headers=HEADERS)
        print(f"Status: {response.status_code}")
        print(f"Response: {response.json()}")
        return response.status_code == 200
    except Exception as e:
        print(f"Error: {e}")
        return False

def test_search_by_id(image_id, model="efficientnet_b0"):
    print(f"\n--- Testing Search by ID ({image_id}) using {model} ---")
    try:
        response = requests.get(
            f"{API_BASE_URL}/search/by-id/{image_id}",
            headers=HEADERS,
            params={"model": model, "limit": 3}
        )
        print(f"Status: {response.status_code}")
        if response.status_code == 200:
            results = response.json()
            print(f"Found {results['count']} results.")
            for i, res in enumerate(results['results']):
                print(f"  {i+1}. Product: {res['product_id']} | Score: {res['score']:.4f}")
        else:
            print(f"Error Response: {response.text}")
        return response.status_code == 200
    except Exception as e:
        print(f"Error: {e}")
        return False

def test_recommendations(product_id, model="efficientnet_b0"):
    print(f"\n--- Testing Recommendations by Product ID ({product_id}) using {model} ---")
    try:
        response = requests.get(
            f"{API_BASE_URL}/recommendations/by-product-id/{product_id}",
            headers=HEADERS,
            params={"model": model, "limit": 3}
        )
        print(f"Status: {response.status_code}")
        if response.status_code == 200:
            results = response.json()
            print(f"Found {results['count']} recommendations.")
            for i, res in enumerate(results['results']):
                print(f"  {i+1}. Product: {res['product_id']} | Score: {res['score']:.4f}")
        else:
            print(f"Error Response: {response.text}")
        return response.status_code == 200
    except Exception as e:
        print(f"Error: {e}")
        return False

def test_search_by_upload(image_path, model="efficientnet_b0"):
    print(f"\n--- Testing Search by Upload ({image_path}) using {model} ---")
    if not os.path.exists(image_path):
        print(f"Skip: Image not found at {image_path}")
        return False
        
    try:
        with open(image_path, "rb") as f:
            files = {"file": (os.path.basename(image_path), f, "image/jpeg")}
            response = requests.post(
                f"{API_BASE_URL}/search/by-upload",
                headers=HEADERS,
                params={"model": model, "limit": 3},
                files=files
            )
        print(f"Status: {response.status_code}")
        if response.status_code == 200:
            results = response.json()
            print(f"Found {results['count']} results.")
            for i, res in enumerate(results['results']):
                print(f"  {i+1}. Product: {res['product_id']} | Score: {res['score']:.4f}")
        else:
            print(f"Error Response: {response.text}")
        return response.status_code == 200
    except Exception as e:
        print(f"Error: {e}")
        return False

if __name__ == "__main__":
    # Get a sample product and image from DB to test with
    sys.path.append(str(Path(__file__).resolve().parent.parent))
    from app.database import SessionLocal, Product, ProductImage
    
    db = SessionLocal()
    sample_product = db.query(Product).first()
    sample_image = db.query(ProductImage).filter(ProductImage.embedding_efficientnet != None).first()
    db.close()
    
    if not sample_product or not sample_image:
        print("Error: Could not find sample data in database. Load data first.")
        sys.exit(1)
        
    print(f"Using Sample Product: {sample_product.id}")
    print(f"Using Sample Image: {sample_image.id} (Path: {sample_image.url})")
    
    test_health()
    test_list_models()
    test_search_by_id(sample_image.id)
    test_recommendations(sample_product.id)
    
    # Try searching with an actual image file from the data directory
    test_search_by_upload(sample_image.url)

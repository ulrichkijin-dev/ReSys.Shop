import requests
import os
import sys
import logging
from pathlib import Path
from typing import Optional, Dict, Any

# Configure logging
logging.basicConfig(level=logging.INFO, format="%(message)s")
logger = logging.getLogger(__name__)

# Configuration
API_BASE_URL = os.getenv("API_BASE_URL", "http://127.0.0.1:8000")
API_KEY = os.getenv("API_KEY", "thesis-secure-api-key-2025")
HEADERS = {"X-API-Key": API_KEY}

def print_header(text: str):
    print("\n" + "=" * 70)
    print(f" {text}")
    print("=" * 70)

def test_root():
    """Test the root endpoint for API documentation/info."""
    print_header("Testing: Root / Info")
    try:
        response = requests.get(f"{API_BASE_URL}/")
        print(f"Status: {response.status_code}")
        if response.status_code == 200:
            data = response.json()
            print(f"✓ Service: {data.get('service')}")
            print(f"✓ Version: {data.get('version')}")
            print(f"✓ Available Models: {len(data.get('available_models', []))}")
            return True
        return False
    except Exception as e:
        print(f"✗ Error: {e}")
        return False

def test_health():
    """Test health check endpoint."""
    print_header("Testing: Health Check")
    try:
        response = requests.get(f"{API_BASE_URL}/health")
        print(f"Status: {response.status_code}")

        if response.status_code == 200:
            data = response.json()
            print(f"✓ Database: {data['database']}")
            print(f"✓ Indexed Images: {data['indexed_images']}")
            print("✓ Models Status:")
            for model in data["models"]:
                status = "✓" if model["loaded"] else "✗"
                print(
                    f"  {status} {model['model_name']}: {model['architecture']}"
                )
            return True
        else:
            print(f"✗ Health check failed: {response.text}")
            return False
    except Exception as e:
        print(f"✗ Error: {e}")
        return False

def test_list_models():
    """Test list models endpoint."""
    print_header("Testing: List Models")
    try:
        response = requests.get(f"{API_BASE_URL}/models", headers=HEADERS)
        print(f"Status: {response.status_code}")

        if response.status_code == 200:
            data = response.json()
            print(f"✓ Total Models: {data['total_models']}")
            print(f"✓ Default Model: {data['default_model']}")
            return True
        else:
            print(f"✗ Failed: {response.text}")
            return False
    except Exception as e:
        print(f"✗ Error: {e}")
        return False

def test_search_by_id(image_id, model):
    """Test search by image ID."""
    print_header(f"Testing: Search by ID ({model})")
    try:
        response = requests.get(
            f"{API_BASE_URL}/search/by-id/{image_id}",
            headers=HEADERS,
            params={"model": model, "limit": 3},
        )
        print(f"Status: {response.status_code}")

        if response.status_code == 200:
            results = response.json()
            print(f"✓ Found {results['count']} similar items")
            for i, res in enumerate(results["results"], 1):
                print(f"  {i}. Score: {res['score']:.4f} | Category: {res.get('article_type')}")
            return True
        else:
            print(f"✗ Error: {response.text}")
            return False
    except Exception as e:
        print(f"✗ Error: {e}")
        return False

def test_recommendations(product_id, model):
    """Test product recommendations."""
    print_header(f"Testing: Recommendations ({model})")
    try:
        response = requests.get(
            f"{API_BASE_URL}/recommendations/by-product-id/{product_id}",
            headers=HEADERS,
            params={"model": model, "limit": 3},
        )
        print(f"Status: {response.status_code}")

        if response.status_code == 200:
            results = response.json()
            print(f"✓ Found {results['count']} recommendations")
            return True
        else:
            print(f"✗ Error: {response.text}")
            return False
    except Exception as e:
        print(f"✗ Error: {e}")
        return False

def test_search_by_upload(image_path, model):
    """Test search by uploading an image."""
    print_header(f"Testing: Search by Upload ({model})")
    if not os.path.exists(image_path):
        print(f"⚠ Skip: Image not found at {image_path}")
        return None

    try:
        with open(image_path, "rb") as f:
            files = {"file": (os.path.basename(image_path), f, "image/jpeg")}
            response = requests.post(
                f"{API_BASE_URL}/search/by-upload",
                headers=HEADERS,
                params={"model": model, "limit": 3},
                files=files,
            )
        print(f"Status: {response.status_code}")

        if response.status_code == 200:
            results = response.json()
            print(f"✓ Found {results['count']} items")
            return True
        else:
            print(f"✗ Error: {response.text}")
            return False
    except Exception as e:
        print(f"✗ Error: {e}")
        return False

def main():
    print_header("Fashion Image Search API - Comprehensive Integration Tests")
    print(f"API Base URL: {API_BASE_URL}")

    # Setup path to import app modules
    sys.path.append(str(Path(__file__).resolve().parent.parent))
    
    try:
        from app.database import SessionLocal, Product, ProductImage
        from app.config import settings
        
        db = SessionLocal()
        # Corrected SQLAlchemy filter syntax
        sample_image = (
            db.query(ProductImage)
            .filter(
                ProductImage.type == "Search",
                ProductImage.embedding_efficientnet != None,
            )
            .first()
        )

        if not sample_image:
            print("\n✗ Error: No sample data with embeddings found in database.")
            return

        product_id = sample_image.product_id
        image_id = sample_image.id
        image_path = sample_image.url
        db.close()

        print(f"\nUsing Sample Data:")
        print(f"  Product ID: {product_id}")
        print(f"  Image ID: {image_id}")
        print(f"  Image Path: {image_path}")

        results = {}
        results["root"] = test_root()
        results["health"] = test_health()
        results["models"] = test_list_models()

        champions = settings.AVAILABLE_MODELS
        for model in champions:
            results[f"search_{model}"] = test_search_by_id(image_id, model)
            results[f"rec_{model}"] = test_recommendations(product_id, model)
            
        # Test upload once
        results["upload"] = test_search_by_upload(image_path, champions[0])

        # Summary
        print_header("TEST SUMMARY")
        passed = sum(1 for v in results.values() if v is True)
        failed = sum(1 for v in results.values() if v is False)
        print(f"Total: {len(results)} | Passed: {passed} | Failed: {failed}")

        if failed > 0:
            sys.exit(1)
            
    except Exception as e:
        print(f"✗ Critical Error: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main()
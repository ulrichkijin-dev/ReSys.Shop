"""
API Testing Script

Manual tests for the Image Search API endpoints.
Run this after starting the API server with: uvicorn app.main:app --reload
"""

import requests
import os
import sys
from pathlib import Path
from typing import Optional

# Configuration
API_BASE_URL = os.getenv("API_BASE_URL", "http://127.0.0.1:8000")
API_KEY = os.getenv("API_KEY", "thesis-secure-api-key-2025")
HEADERS = {"X-API-Key": API_KEY}


def print_section(title):
    """Print formatted section header."""
    print("\n" + "="*70)
    print(f" {title}")
    print("="*70)


def test_root():
    """Test root endpoint."""
    print_section("Testing Root Endpoint")
    try:
        response = requests.get(f"{API_BASE_URL}/")
        print(f"Status: {response.status_code}")
        if response.status_code == 200:
            data = response.json()
            print(f"✓ Service: {data['service']}")
            print(f"✓ Version: {data['version']}")
            print(f"✓ Available models: {', '.join(data['available_models'])}")
            return True
        else:
            print(f"✗ Unexpected status code")
            return False
    except Exception as e:
        print(f"✗ Error: {e}")
        return False


def test_health():
    """Test health check endpoint."""
    print_section("Testing Health Check")
    try:
        response = requests.get(f"{API_BASE_URL}/health")
        print(f"Status: {response.status_code}")
        if response.status_code == 200:
            data = response.json()
            print(f"✓ Status: {data['status']}")
            print(f"✓ Database: {data['database']}")
            print(f"✓ Indexed images: {data['indexed_images']}")
            print("\nModel Status:")
            for model in data['models']:
                status = "✓" if model['loaded'] else "✗"
                if model['loaded']:
                    print(f"  {status} {model['model_name']}: {model['architecture']} (dim={model['dimensions']})")
                else:
                    print(f"  {status} {model['model_name']}: {model.get('error', 'Not loaded')}")
            return True
        else:
            print(f"✗ Unexpected status code")
            return False
    except Exception as e:
        print(f"✗ Error: {e}")
        return False


def test_list_models():
    """Test list models endpoint."""
    print_section("Testing List Models")
    try:
        response = requests.get(f"{API_BASE_URL}/models", headers=HEADERS)
        print(f"Status: {response.status_code}")
        if response.status_code == 200:
            data = response.json()
            print(f"✓ Total models: {data['total_models']}")
            print(f"✓ Default model: {data['default_model']}")
            print("\nDetailed Model Info:")
            for name, info in data['models'].items():
                if info['loaded']:
                    print(f"  ✓ {name}")
                    print(f"     Architecture: {info['architecture']}")
                    print(f"     Dimensions: {info['dimensions']}")
                else:
                    print(f"  ✗ {name}: {info.get('error', 'Not loaded')}")
            return True
        else:
            print(f"✗ Response: {response.text}")
            return False
    except Exception as e:
        print(f"✗ Error: {e}")
        return False


def test_search_by_id(image_id: str, model: str = "efficientnet_b0"):
    """Test search by image ID."""
    print_section(f"Testing Search by ID ({model})")
    try:
        response = requests.get(
            f"{API_BASE_URL}/search/by-id/{image_id}",
            headers=HEADERS,
            params={"model": model, "limit": 5}
        )
        print(f"Status: {response.status_code}")
        if response.status_code == 200:
            data = response.json()
            print(f"✓ Model: {data['model']}")
            print(f"✓ Results found: {data['count']}")
            print("\nTop Results:")
            for i, result in enumerate(data['results'][:3], 1):
                print(f"  {i}. Product: {result['product_id']}")
                print(f"     Score: {result['score']:.4f}")
                print(f"     Category: {result['article_type']} / {result['sub_category']}")
            return True
        else:
            print(f"✗ Error: {response.text}")
            return False
    except Exception as e:
        print(f"✗ Error: {e}")
        return False


def test_recommendations(product_id: str, model: str = "efficientnet_b0"):
    """Test product recommendations."""
    print_section(f"Testing Recommendations ({model})")
    try:
        response = requests.get(
            f"{API_BASE_URL}/recommendations/by-product-id/{product_id}",
            headers=HEADERS,
            params={"model": model, "limit": 5}
        )
        print(f"Status: {response.status_code}")
        if response.status_code == 200:
            data = response.json()
            print(f"✓ Source product: {data['source_product_id']}")
            print(f"✓ Model: {data['model']}")
            print(f"✓ Recommendations found: {data['count']}")
            print("\nTop Recommendations:")
            for i, result in enumerate(data['results'][:3], 1):
                print(f"  {i}. Product: {result['product_id']}")
                print(f"     Score: {result['score']:.4f}")
                print(f"     Category: {result['article_type']}")
            return True
        else:
            print(f"✗ Error: {response.text}")
            return False
    except Exception as e:
        print(f"✗ Error: {e}")
        return False


def test_search_by_upload(image_path: str, model: str = "efficientnet_b0"):
    """Test search by uploading an image."""
    print_section(f"Testing Search by Upload ({model})")
    
    if not os.path.exists(image_path):
        print(f"✗ Image not found: {image_path}")
        return False
    
    try:
        with open(image_path, "rb") as f:
            files = {"file": (os.path.basename(image_path), f, "image/jpeg")}
            response = requests.post(
                f"{API_BASE_URL}/search/by-upload",
                headers=HEADERS,
                params={"model": model, "limit": 5},
                files=files
            )
        
        print(f"Status: {response.status_code}")
        if response.status_code == 200:
            data = response.json()
            print(f"✓ Model: {data['model']}")
            print(f"✓ Results found: {data['count']}")
            print("\nTop Results:")
            for i, result in enumerate(data['results'][:3], 1):
                print(f"  {i}. Product: {result['product_id']}")
                print(f"     Score: {result['score']:.4f}")
                print(f"     Category: {result['article_type']}")
            return True
        else:
            print(f"✗ Error: {response.text}")
            return False
    except Exception as e:
        print(f"✗ Error: {e}")
        return False


def test_compare_models(image_id: str):
    """Test model comparison endpoint."""
    print_section("Testing Model Comparison")
    try:
        response = requests.get(
            f"{API_BASE_URL}/diagnostics/compare-models/{image_id}",
            headers=HEADERS,
            params={
                "models": ["mobilenet_v3", "efficientnet_b0", "clip"],
                "limit": 3
            }
        )
        print(f"Status: {response.status_code}")
        if response.status_code == 200:
            data = response.json()
            print(f"✓ Source image: {data['source_image_id']}")
            print("\nResults by Model:")
            for model_name, results in data['results_by_model'].items():
                print(f"\n  {model_name}:")
                print(f"    Results: {results['count']}")
                if results['results']:
                    for i, r in enumerate(results['results'][:2], 1):
                        print(f"      {i}. Score: {r['score']:.4f}, Category: {r['article_type']}")
            return True
        else:
            print(f"✗ Error: {response.text}")
            return False
    except Exception as e:
        print(f"✗ Error: {e}")
        return False


def get_sample_data():
    """Get sample product and image IDs from database."""
    sys.path.append(str(Path(__file__).resolve().parent.parent))
    try:
        from app.database import SessionLocal, Product, ProductImage
        
        db = SessionLocal()
        try:
            sample_product = db.query(Product).first()
            sample_image = db.query(ProductImage).filter(
                ProductImage.embedding_efficientnet != None,
                ProductImage.type == 'Search'
            ).first()
            
            if not sample_product or not sample_image:
                return None, None, None
            
            return (
                str(sample_product.id),
                str(sample_image.id),
                sample_image.url
            )
        finally:
            db.close()
    except Exception as e:
        print(f"Error getting sample data: {e}")
        return None, None, None


def main():
    """Run all API tests."""
    print("\n" + "="*70)
    print(" Image Search API - Integration Tests")
    print("="*70)
    print(f"\nAPI URL: {API_BASE_URL}")
    print("Make sure the API server is running:")
    print("  uvicorn app.main:app --reload\n")
    
    # Get sample data
    product_id, image_id, image_path = get_sample_data()
    
    if not product_id or not image_id:
        print("⚠ Warning: No sample data found in database")
        print("Load data first: python -m app.dataset_loader --json data/styles.csv --images data/images --total 100 --clear")
        print("\nRunning basic tests only...")
    
    # Run tests
    results = {}
    
    # Basic tests (no auth required)
    results['Root'] = test_root()
    results['Health'] = test_health()
    
    # Authenticated tests
    results['List Models'] = test_list_models()
    
    # Tests requiring data
    if image_id:
        results['Search by ID'] = test_search_by_id(image_id)
        results['Model Comparison'] = test_compare_models(image_id)
    
    if product_id:
        results['Recommendations'] = test_recommendations(product_id)
    
    if image_path and os.path.exists(image_path):
        results['Search by Upload'] = test_search_by_upload(image_path)
    
    # Summary
    print_section("Test Summary")
    passed = sum(1 for v in results.values() if v)
    total = len(results)
    
    for test_name, result in results.items():
        status = "✓ PASSED" if result else "✗ FAILED"
        print(f"{status}: {test_name}")
    
    print(f"\nResults: {passed}/{total} tests passed")
    
    if passed == total:
        print("\n✓ All tests passed!")
        return 0
    else:
        print(f"\n✗ {total - passed} test(s) failed")
        return 1


if __name__ == "__main__":
    sys.exit(main())
import requests
import os
import uuid
import sys
from pathlib import Path

# Configuration
API_BASE_URL = os.getenv("API_BASE_URL", "http://127.0.0.1:8000")
API_KEY = os.getenv("API_KEY", "thesis-secure-api-key-2025")
HEADERS = {"X-API-Key": API_KEY}


def test_health():
    """Test health check endpoint."""
    print("\n" + "=" * 70)
    print(" Testing: Health Check")
    print("=" * 70)
    try:
        response = requests.get(f"{API_BASE_URL}/health")
        print(f"Status: {response.status_code}")

        if response.status_code == 200:
            data = response.json()
            print(f"✓ Database: {data['database']}")
            print(f"✓ Indexed Images: {data['indexed_images']}")
            print(f"✓ Models Loaded:")
            for model in data["models"]:
                status = "✓" if model["loaded"] else "✗"
                print(
                    f"  {status} {model['model_name']} ({model['architecture']}, {model['dimensions']} dims)"
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
    print("\n" + "=" * 70)
    print(" Testing: List Models")
    print("=" * 70)
    try:
        response = requests.get(f"{API_BASE_URL}/models", headers=HEADERS)
        print(f"Status: {response.status_code}")

        if response.status_code == 200:
            data = response.json()
            print(f"✓ Total Models: {data['total_models']}")
            print(f"✓ Default Model: {data['default_model']}")
            print(f"✓ Available Models:")
            for name, info in data["models"].items():
                status = "✓" if info["loaded"] else "✗"
                print(
                    f"  {status} {name} ({info['architecture']}, {info['dimensions']} dims)"
                )
            return True
        else:
            print(f"✗ Failed: {response.text}")
            return False
    except Exception as e:
        print(f"✗ Error: {e}")
        return False


def test_search_by_id(image_id, model="efficientnet_b0"):
    """Test search by image ID."""
    print("\n" + "=" * 70)
    print(f" Testing: Search by ID using {model}")
    print("=" * 70)
    print(f"Image ID: {image_id}")

    try:
        response = requests.get(
            f"{API_BASE_URL}/search/by-id/{image_id}",
            headers=HEADERS,
            params={"model": model, "limit": 5},
        )
        print(f"Status: {response.status_code}")

        if response.status_code == 200:
            results = response.json()
            print(f"✓ Found {results['count']} similar items")
            print(f"✓ Results:")
            for i, res in enumerate(results["results"], 1):
                print(f"  {i}. Product: {res['product_id']}")
                print(f"     Score: {res['score']:.4f}")
                print(f"     Category: {res.get('article_type', 'N/A')}")
            return True
        else:
            print(f"✗ Error: {response.text}")
            return False
    except Exception as e:
        print(f"✗ Error: {e}")
        return False


def test_recommendations(product_id, model="efficientnet_b0"):
    """Test product recommendations."""
    print("\n" + "=" * 70)
    print(f" Testing: Recommendations using {model}")
    print("=" * 70)
    print(f"Product ID: {product_id}")

    try:
        response = requests.get(
            f"{API_BASE_URL}/recommendations/by-product-id/{product_id}",
            headers=HEADERS,
            params={"model": model, "limit": 5},
        )
        print(f"Status: {response.status_code}")

        if response.status_code == 200:
            results = response.json()
            print(f"✓ Found {results['count']} recommendations")
            print(f"✓ Results:")
            for i, res in enumerate(results["results"], 1):
                print(f"  {i}. Product: {res['product_id']}")
                print(f"     Score: {res['score']:.4f}")
                print(f"     Category: {res.get('article_type', 'N/A')}")
            return True
        else:
            print(f"✗ Error: {response.text}")
            return False
    except Exception as e:
        print(f"✗ Error: {e}")
        return False


def test_search_by_upload(image_path, model="efficientnet_b0"):
    """Test search by uploading an image."""
    print("\n" + "=" * 70)
    print(f" Testing: Search by Upload using {model}")
    print("=" * 70)
    print(f"Image: {image_path}")

    if not os.path.exists(image_path):
        print(f"⚠ Skip: Image not found at {image_path}")
        return None

    try:
        with open(image_path, "rb") as f:
            files = {"file": (os.path.basename(image_path), f, "image/jpeg")}
            response = requests.post(
                f"{API_BASE_URL}/search/by-upload",
                headers=HEADERS,
                params={"model": model, "limit": 5},
                files=files,
            )
        print(f"Status: {response.status_code}")

        if response.status_code == 200:
            results = response.json()
            print(f"✓ Found {results['count']} similar items")
            print(f"✓ Results:")
            for i, res in enumerate(results["results"], 1):
                print(f"  {i}. Product: {res['product_id']}")
                print(f"     Score: {res['score']:.4f}")
                print(f"     Category: {res.get('article_type', 'N/A')}")
            return True
        else:
            print(f"✗ Error: {response.text}")
            return False
    except Exception as e:
        print(f"✗ Error: {e}")
        return False


def test_compare_models(image_id):
    """Test model comparison endpoint."""
    print("\n" + "=" * 70)
    print(f" Testing: Compare Models")
    print("=" * 70)
    print(f"Image ID: {image_id}")

    try:
        from app.config import settings

        models = settings.AVAILABLE_MODELS

        response = requests.get(
            f"{API_BASE_URL}/diagnostics/compare-models/{image_id}",
            headers=HEADERS,
            params={"models": models, "limit": 3},
        )
        print(f"Status: {response.status_code}")

        if response.status_code == 200:
            results = response.json()
            print(f"✓ Comparison Results:")
            for model, data in results["results_by_model"].items():
                if "error" in data:
                    print(f"  ✗ {model}: {data['error']}")
                else:
                    print(f"  ✓ {model}: {data['count']} results")
            return True
        else:
            print(f"✗ Error: {response.text}")
            return False
    except Exception as e:
        print(f"✗ Error: {e}")
        return False


if __name__ == "__main__":
    print("\n" + "=" * 70)
    print(" Fashion Image Search API - Integration Tests")
    print(" Testing 5 Champion Models")
    print("=" * 70)
    print(f"API Base URL: {API_BASE_URL}")

    # Get sample data from database
    sys.path.append(str(Path(__file__).resolve().parent.parent))

    try:
        from app.database import SessionLocal, Product, ProductImage
        from app.config import settings

        db = SessionLocal()

        # Get a sample product with embeddings
        sample_image = (
            db.query(ProductImage)
            .filter(
                ProductImage.type == "Search",
                ProductImage.embedding_efficientnet != None,
            )
            .first()
        )

        if sample_image:
            sample_product = (
                db.query(Product).filter(Product.id == sample_image.product_id).first()
            )
        else:
            sample_product = None

        db.close()

        if not sample_product or not sample_image:
            print("\n✗ Error: Could not find sample data in database.")
            print("Please load data first:")
            print(
                "  python -m app.dataset_loader --json data/styles.csv --images data/images --total 1000"
            )
            sys.exit(1)

        print(f"\nUsing Sample Data:")
        print(f"  Product ID: {sample_product.id}")
        print(f"  Image ID: {sample_image.id}")
        print(f"  Image Path: {sample_image.url}")

    except Exception as e:
        print(f"\n✗ Error accessing database: {e}")
        sys.exit(1)

    # Run tests
    results = {}

    # Basic tests
    results["health"] = test_health()
    results["list_models"] = test_list_models()

    # Test all 5 champion models
    champions = settings.AVAILABLE_MODELS
    print(f"\n{'=' * 70}")
    print(f" Testing All {len(champions)} Champion Models")
    print(f"{'=' * 70}")

    for model in champions:
        print(f"\n{'─' * 70}")
        print(f" MODEL: {model}")
        print(f"{'─' * 70}")

        results[f"search_{model}"] = test_search_by_id(sample_image.id, model=model)
        results[f"rec_{model}"] = test_recommendations(sample_product.id, model=model)

        # Test upload with first image only
        if model == champions[0]:
            results[f"upload_{model}"] = test_search_by_upload(
                sample_image.url, model=model
            )

    # Test comparison endpoint
    results["compare"] = test_compare_models(sample_image.id)

    # Summary
    print("\n" + "=" * 70)
    print(" TEST SUMMARY")
    print("=" * 70)

    passed = sum(1 for v in results.values() if v is True)
    failed = sum(1 for v in results.values() if v is False)
    skipped = sum(1 for v in results.values() if v is None)
    total = len(results)

    print(f"Total Tests: {total}")
    print(f"✓ Passed: {passed}")
    print(f"✗ Failed: {failed}")
    print(f"⚠ Skipped: {skipped}")

    if failed == 0:
        print("\n✓ All tests passed! API is working correctly.")
        sys.exit(0)
    else:
        print("\n✗ Some tests failed. Check the output above for details.")
        sys.exit(1)

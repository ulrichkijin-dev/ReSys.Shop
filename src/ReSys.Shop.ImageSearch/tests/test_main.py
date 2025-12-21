import os
import uuid
from fastapi.testclient import TestClient
from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker
import pytest
from PIL import Image
import io

# Add app directory to path to import app and database
import sys
from pathlib import Path
sys.path.append(str(Path(__file__).parent.parent / "app"))

from main import app, get_db, API_KEY, API_KEY_NAME
from database import Base, Product, ProductImage

# --- Test Database Setup ---
TEST_DATABASE_URL = os.getenv("TEST_DATABASE_URL", "postgresql://postgres:password@localhost:5432/EshopTestDb")
engine = create_engine(TEST_DATABASE_URL)
TestingSessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)

# Create tables for the test DB, if they don't exist
# In a real-world scenario, you'd use Alembic migrations
Base.metadata.create_all(bind=engine)

def override_get_db():
    try:
        db = TestingSessionLocal()
        yield db
    finally:
        db.close()

app.dependency_overrides[get_db] = override_get_db

client = TestClient(app)
api_headers = {API_KEY_NAME: API_KEY}

# --- Fixtures ---
@pytest.fixture(scope="function")
def db_session():
    """
    Yields a DB session for a single test function.
    Changed to 'function' scope for test isolation, as some tests modify data.
    """
    db = TestingSessionLocal()
    try:
        yield db
    finally:
        db.rollback() # Rollback any changes after test
        db.close()

@pytest.fixture(scope="function")
def sample_data(db_session):
    """Provides sample data from the DB for testing."""
    test_image = db_session.query(ProductImage).filter(ProductImage.type == "Search").first()
    assert test_image is not None, "Test DB is empty. Please load data first."
    
    test_product = db_session.query(Product).get(test_image.product_id)
    assert test_product is not None, "Test DB has orphaned images."
    
    return {"image": test_image, "product": test_product}

# --- Test Cases ---

def test_health_check():
    response = client.get("/health")
    assert response.status_code == 200
    data = response.json()
    assert data["status"] == "healthy"
    assert data["database"] == "connected"

def test_list_models():
    response = client.get("/models", headers=api_headers)
    assert response.status_code == 200
    data = response.json()
    assert data["total_models"] == 3
    assert "clip" in data["models"]

def test_generate_embeddings_batch(db_session, sample_data):
    # 1. Setup: Pick an image and ensure one of its embeddings is NULL
    image_to_process = sample_data["image"]
    image_id_str = str(image_to_process.id)
    
    image_to_process.embedding_clip = None
    db_session.commit()

    # 2. Call the new endpoint
    request_body = {"image_ids": [image_id_str]}
    response = client.post("/embeddings/generate", headers=api_headers, json=request_body)
    
    # 3. Assert the response
    assert response.status_code == 200
    data = response.json()
    assert data["successful"] == 1
    assert data["failed"] == 0
    assert data["details"][0]["image_id"] == image_id_str
    assert data["details"][0]["status"] == "success"
    assert "clip" in data["details"][0]["updated_embeddings"]

    # 4. Verify the change in the database
    db_session.refresh(image_to_process)
    assert image_to_process.embedding_clip is not None
    assert len(image_to_process.embedding_clip) == 512 # CLIP model dimension

def test_generate_embeddings_batch_empty_list():
    response = client.post("/embeddings/generate", headers=api_headers, json={"image_ids": []})
    assert response.status_code == 200
    data = response.json()
    assert data["successful"] == 0
    assert data["failed"] == 0
    assert len(data["details"]) == 0

def test_search_by_upload_success():
    # Create a dummy image file in memory
    img = Image.new('RGB', (100, 100), color = 'red')
    img_byte_arr = io.BytesIO()
    img.save(img_byte_arr, format='JPEG')
    img_byte_arr.seek(0)

    response = client.post(
        "/search/by-upload",
        headers=api_headers,
        params={"model": "clip", "limit": 5},
        files={"file": ("test.jpg", img_byte_arr, "image/jpeg")}
    )
    assert response.status_code == 200
    data = response.json()
    assert data["model"] == "clip"
    assert data["count"] <= 5

def test_search_by_id_success(sample_data):
    image_id = sample_data["image"].id
    # Ensure the embedding exists before searching
    if sample_data["image"].embedding_clip is None:
        pytest.skip("Sample image is missing the required CLIP embedding for this test.")

    response = client.get(f"/search/by-id/{image_id}", headers=api_headers, params={"model": "clip", "limit": 5})
    assert response.status_code == 200
    data = response.json()
    assert data["model"] == "clip"
    assert data["count"] <= 5
    assert len(data["results"]) <= 5
    for item in data["results"]:
        assert "product_id" in item
        assert "score" in item
        assert 0 <= item["score"] <= 1

def test_search_by_id_not_found():
    non_existent_id = uuid.uuid4()
    response = client.get(f"/search/by-id/{non_existent_id}", headers=api_headers)
    assert response.status_code == 404
    assert "Source image not found" in response.json()["detail"]

def test_search_by_id_invalid_model(sample_data):
    image_id = sample_data["image"].id
    response = client.get(f"/search/by-id/{image_id}", headers=api_headers, params={"model": "invalid_model"})
    assert response.status_code == 422 # Unprocessable Entity for invalid enum value

def test_recommendations_by_product_id_success(sample_data):
    product_id = sample_data["product"].id
    response = client.get(f"/recommendations/by-product-id/{product_id}", headers=api_headers, params={"model": "clip", "limit": 5})
    assert response.status_code == 200
    data = response.json()
    assert data["model"] == "clip"
    assert data["source_product_id"] == str(product_id)
    assert len(data["results"]) <= 5
    
    # Ensure it's not recommending the same product
    for item in data["results"]:
        assert item["product_id"] != str(product_id)

def test_recommendations_by_product_id_not_found():
    non_existent_id = uuid.uuid4()
    response = client.get(f"/recommendations/by-product-id/{non_existent_id}", headers=api_headers)
    assert response.status_code == 404
    assert "Source product not found" in response.json()["detail"]

def test_compare_models_success(sample_data):
    image_id = sample_data["image"].id
    models_to_compare = ["clip", "mobilenet_v3"]
    response = client.get(
        f"/diagnostics/compare-models/{image_id}",
        headers=api_headers,
        params={"models": models_to_compare, "limit": 3}
    )
    assert response.status_code == 200
    data = response.json()
    assert data["source_image_id"] == str(image_id)
    assert "results_by_model" in data
    
    results_by_model = data["results_by_model"]
    assert set(results_by_model.keys()) == set(models_to_compare)
    
    for model_name, result in results_by_model.items():
        assert result["model"] == model_name
        assert result["count"] <= 3
        assert "results" in result
        for item in result["results"]:
            assert "product_id" in item
            assert "score" in item

def test_compare_models_with_missing_embedding(db_session, sample_data):
    image_id = sample_data["image"].id
    
    # Manually remove an embedding to test this case
    image_to_process = sample_data["image"]
    image_to_process.embedding_mobilenet = None
    db_session.commit()

    models_to_compare = ["clip", "mobilenet_v3"]
    response = client.get(
        f"/diagnostics/compare-models/{image_id}",
        headers=api_headers,
        params={"models": models_to_compare, "limit": 3}
    )
    assert response.status_code == 200
    data = response.json()
    
    # The result for the model with the missing embedding should be empty
    mobilenet_result = data["results_by_model"]["mobilenet_v3"]
    assert mobilenet_result["count"] == 0
    assert len(mobilenet_result["results"]) == 0
    
    # The result for the other model should be present
    clip_result = data["results_by_model"]["clip"]
    assert clip_result["count"] <= 3

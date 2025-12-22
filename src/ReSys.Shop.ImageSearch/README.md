# ReSys.Shop Image Search Service

The ReSys.Shop Image Search is a Python-based microservice designed to provide powerful visual search and recommendation capabilities for the e-commerce platform. It uses deep learning models to transform product images into vector embeddings, allowing for fast and accurate similarity search.

This service is built with FastAPI and leverages the `pgvector` extension for PostgreSQL to store and query high-dimensional vector data efficiently.

## Architecture

The service consists of the following key components:
1.  **FastAPI Application**: A high-performance Python web framework for building the API endpoints.
2.  **PostgreSQL + pgvector**: The primary database for storing product metadata and vector embeddings. `pgvector` adds the ability to perform efficient vector similarity searches directly in the database.
3.  **Embedding Models**: A factory (`model_factory.py`) provides multiple deep learning models for generating image embeddings. This project is set up to compare:
    *   **MobileNetV3**: An efficient CNN focused on speed.
    *   **EfficientNet**: A scaled CNN balancing accuracy and efficiency.
    *   **CLIP**: A transformer-based model from OpenAI that understands both images and text, providing strong semantic search capabilities.
4.  **Data Loader**: A script (`dataset_loader.py`) to populate the database with product data from a JSON file and pre-compute embeddings for all images.

---

## Getting Started

### Prerequisites
- Python 3.9+
- PostgreSQL server with the **pgvector** extension installed.
- Access to the Fashion Product Images dataset (or a similar one).

### 1. Installation

It is highly recommended to use a Python virtual environment.

```bash
# Navigate to this directory
cd src/ReSys.Shop.ImageSearch

# Create a virtual environment (optional but recommended)
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate

# Install dependencies
pip install -r requirements.txt
```

### 2. Database Setup
The service requires a PostgreSQL database with the `pgvector` extension enabled.

1.  **Configure the connection string**. The application uses the `DATABASE_URL` environment variable or a `.env` file. The default is set to:
    `postgresql://postgres:12345678@localhost:5432/eshopdb`

2.  **Download the dataset**. We recommend using the Kaggle CLI to download the "Fashion Product Images (Small)" dataset.

    ```bash
    # Ensure you have kaggle API credentials set up
    # https://github.com/Kaggle/kaggle-api
    kaggle datasets download -d paramaggarwal/fashion-product-images-small
    
    # Extract the dataset
    unzip fashion-product-images-small.zip -d data/
    ```

3.  **Load the dataset**. Populate the database using the dataset loader. This script will create the tables, enable `pgvector`, load metadata (CSV or JSON), and compute embeddings.

    ```bash
    # For the small dataset (CSV metadata)
    python -m app.dataset_loader --json data/styles.csv --images data/images --total 4000
    ```

---

## Running the Service

Once the dependencies are installed and the database is populated, start the API server:

```bash
# Start the server (uvicorn will load .env automatically if present)
uvicorn app.main:app --reload
```

The API will be available at `http://127.0.0.1:8000`. Explore the Swagger documentation at `http://127.0.0.1:8000/docs`.

### Quick health & models checks

```bash
# Health check (no API key required)
curl http://127.0.0.1:8000/health

# List models (requires API key)
curl -H "X-API-Key: thesis-secure-api-key-2025" http://127.0.0.1:8000/models
```

### Seed a minimal dataset (for quick local testing)

If you just want to run the API locally without the full dataset, you can seed a small sample product into the DB:

```bash
# Use SQLite for quick local dev
$env:DEV_SQLITE = "1"
python scripts/seed_sample_data.py

# Then start the API and the sample product will be available for searches
uvicorn app.main:app --reload
```

---

## API Endpoints

The API is secured with an API key. All requests must include the `X-API-Key` header with the configured key.

### Search

#### `GET /search/by-id/{image_id}`
Searches for visually similar images using an existing image ID from the database.

- **Parameters**:
    - `image_id` (UUID, path): The UUID of the query image.
    - `limit` (int, query): The maximum number of results to return. Default: 10.
    - `model` (str, query): The embedding model to use (`clip`, `mobilenet_v3`, `efficientnet_b0`). Default: `clip`.

- **Example Response** (`200 OK`):
  ```json
  {
    "model": "clip",
    "count": 1,
    "results": [
      {
        "product_id": "a1b2c3d4-e5f6-a7b8-c9d0-e1f2a3b4c5d6",
        "image_id": "f1e2d3c4-b5a6-f7e8-d9c0-b1a2f3e4d5c6",
        "image_url": "data/images/1525.jpg",
        "score": 0.9876
      }
    ]
  }
  ```

### Recommendations

#### `GET /recommendations/by-product-id/{product_id}`
Recommends visually similar products based on a given product ID. It uses the product's primary image as the query.

- **Parameters**:
    - `product_id` (UUID, path): The UUID of the product to get recommendations for.
    - `limit` (int, query): The maximum number of recommendations to return. Default: 10.
    - `model` (str, query): The embedding model to use. Default: `clip`.

- **Example Response** (`200 OK`):
  ```json
  {
    "source_product_id": "a1b2c3d4-e5f6-a7b8-c9d0-e1f2a3b4c5d6",
    "model": "clip",
    "count": 1,
    "results": [
      {
        "product_id": "b2c3d4e5-f6a7-b8c9-d0e1-f2a3b4c5d6e7",
        "image_id": "e2d3c4b5-a6f7-e8d9-c0b1-a2f3e4d5c6f7",
        "image_url": "data/images/1526.jpg",
        "score": 0.9753
      }
    ]
  }
  ```
---

## Testing and Evaluation

### Running the Test Suite
The project includes a test suite to validate the API endpoints.

1.  **Setup the Test Database**: The tests are configured to run against a separate test database (`EshopTestDb` by default). Make sure this database exists, has the `pgvector` extension, and is populated with data. You can configure the connection string via the `TEST_DATABASE_URL` environment variable.

    **Quick local developer mode:** If you don't have Postgres available, you can run tests against a lightweight SQLite file by setting an env var before running tests:

    ```bash
    # On PowerShell
    $env:DEV_SQLITE = "1"
    pytest

    # On Linux/macOS
    DEV_SQLITE=1 pytest
    ```

2.  **Run Pytest**: Navigate to the service directory and run `pytest`.

    ```bash
    # From the src/ReSys.Shop.ImageSearch/ directory
    pytest
    ```

### Evaluating Search Performance
An evaluation script is provided to calculate retrieval metrics (Precision@K and Recall@K). It uses the product's `articleType` as the ground truth for relevance.

1.  **Ensure the API is running**.
2.  **Run the script**:
    ```bash
    # From the root of the repository
    python src/ReSys.Shop.ImageSearch/evaluation/metrics.py
    ```
The script will output a report summarizing the performance of the search model.

---

## Integration with .NET API

This Python service is designed to be consumed by the main `ReSys.Shop.Api` (.NET) application. The .NET backend will act as an HTTP client to this service.

### Configuration
In your .NET application's `appsettings.json`, configure the base URL and API key for the Image Search service.

```json
{
  "ImageSearchService": {
    "BaseUrl": "http://127.0.0.1:8000",
    "ApiKey": "thesis-secure-api-key-2025"
  }
}
```

### C# Client Example
You can create a typed `HttpClient` in your .NET `Startup.cs` or `Program.cs` to interact with the service.

**1. Define a client service:**

```csharp
public class ImageSearchClient
{
    private readonly HttpClient _httpClient;

    public ImageSearchClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<RecommendationResponse?> GetRecommendationsAsync(Guid productId, int limit = 10, string model = "clip")
    {
        var response = await _httpClient.GetAsync($"recommendations/by-product-id/{productId}?limit={limit}&model={model}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RecommendationResponse>();
    }
}

// Define the response model to match the API
public class RecommendationResponse
{
    public Guid SourceProductId { get; set; }
    public string Model { get; set; }
    public int Count { get; set; }
    public List<SearchResultItem> Results { get; set; }
}

public class SearchResultItem
{
    public Guid ProductId { get; set; }
    public Guid ImageId { get; set; }
    public string ImageUrl { get; set; }
    public float Score { get; set; }
}
```

**2. Register the client in `Program.cs` (or `Startup.cs`):**

```csharp
// In your DependencyInjection setup
var imageSearchConfig = builder.Configuration.GetSection("ImageSearchService");
builder.Services.AddHttpClient<ImageSearchClient>(client =>
{
    client.BaseAddress = new Uri(imageSearchConfig["BaseUrl"]);
    client.DefaultRequestHeaders.Add("X-API-Key", imageSearchConfig["ApiKey"]);
});
```

**3. Use the client in your services or controllers:**

```csharp
public class ProductService
{
    private readonly ImageSearchClient _imageSearchClient;

    public ProductService(ImageSearchClient imageSearchClient)
    {
        _imageSearchClient = imageSearchClient;
    }

    public async Task<List<Product>> GetVisualRecommendations(Guid productId)
    {
        var recommendations = await _imageSearchClient.GetRecommendationsAsync(productId);
        
        if (recommendations == null || !recommendations.Results.Any())
        {
            return new List<Product>();
        }

        var recommendedProductIds = recommendations.Results.Select(r => r.ProductId).ToList();
        
        // Now, fetch these products from your main application's database
        // return await _dbContext.Products
        //     .Where(p => recommendedProductIds.Contains(p.Id))
        //     .ToListAsync();
        
        return new List<Product>(); // Placeholder
    }
}
```
This pattern decouples the .NET application from the Python ML service, allowing each to be developed, scaled, and deployed independently.
```
# Fashion Image Search API - Thesis Project

> **Comparative Analysis of CNN vs Transformer Architectures for Visual Product Search**

A production-ready image search system comparing three deep learning architectures:
- **MobileNetV3-Small** (Efficient CNN)
- **EfficientNet-B0** (Scaled CNN)
- **CLIP ViT-B/32** (Vision Transformer)

## 📋 Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [API Endpoints](#api-endpoints)
- [Evaluation Metrics](#evaluation-metrics)
- [Project Structure](#project-structure)
- [Troubleshooting](#troubleshooting)

## ✨ Features

- **Multi-Model Comparison**: Simultaneous support for 3 different embedding architectures
- **Vector Similarity Search**: Fast nearest-neighbor search using pgvector
- **RESTful API**: Complete FastAPI application with automatic documentation
- **Batch Processing**: Efficient embedding generation for large datasets
- **Thesis Metrics**: Built-in P@K, R@K, and mAP@K evaluation
- **Production Ready**: Includes logging, error handling, and health checks

## 🏗️ Architecture

### Model Comparison

| Model | Type | Embedding Dim | Parameters | Use Case |
|-------|------|---------------|------------|----------|
| MobileNetV3-Small | Efficient CNN | 576 | ~2.5M | Mobile/Edge devices |
| EfficientNet-B0 | Scaled CNN | 1280 | ~5.3M | Balanced performance |
| CLIP ViT-B/32 | Vision Transformer | 512 | ~151M | Semantic understanding |

### Tech Stack

- **Backend**: FastAPI 0.104+
- **Database**: PostgreSQL 14+ with pgvector
- **ML Framework**: PyTorch 2.1+
- **Image Processing**: Pillow, torchvision
- **NLP Models**: HuggingFace Transformers

## 📦 Prerequisites

### System Requirements

- Python 3.9+
- PostgreSQL 14+ with pgvector extension
- 8GB RAM minimum (16GB recommended)
- GPU optional (CUDA 11.8+ if available)

### Installing PostgreSQL with pgvector

**Ubuntu/Debian:**
```bash
sudo apt update
sudo apt install postgresql postgresql-contrib
sudo apt install postgresql-16-pgvector
```

**macOS:**
```bash
brew install postgresql@16
brew install pgvector
```

**Verify Installation:**
```bash
psql --version
# PostgreSQL 16.x
```

## 🚀 Installation

### 1. Clone Repository

```bash
git clone <repository-url>
cd fashion-image-search
```

### 2. Create Virtual Environment

```bash
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate
```

### 3. Install Dependencies

```bash
pip install -r requirements.txt
```

### 4. Setup Database

```bash
# Create database
sudo -u postgres psql
CREATE DATABASE eshopdb;
CREATE USER postgres WITH PASSWORD 'your_password';
GRANT ALL PRIVILEGES ON DATABASE eshopdb TO postgres;
\q

# Enable pgvector extension
psql -U postgres -d eshopdb
CREATE EXTENSION vector;
\q
```

### 5. Configure Environment

```bash
cp .env.example .env
# Edit .env with your settings
```

### 6. Validate Setup

```bash
python scripts/validate_setup.py
```

## ⚙️ Configuration

### Environment Variables (.env)

```bash
# Database
DATABASE_URL=postgresql://postgres:your_password@localhost:5432/eshopdb

# API
API_KEY=thesis-secure-api-key-2025
MODEL_TYPE=efficientnet_b0  # Default model

# Paths
UPLOAD_DIR=data/uploads
```

## 📊 Usage

### Step 1: Download Dataset

```bash
# Option 1: Kaggle (recommended)
python scripts/download_dataset.py \
    --kaggle paramaggarwal/fashion-product-images-small \
    --dest data

# Option 2: Manual download
# Download from Kaggle and extract to data/
```

### Step 2: Load Data into Database

```bash
python -m app.dataset_loader \
    --json data/styles.csv \
    --images data/images \
    --total 4000 \
    --clear
```

**Options:**
- `--json`: Path to metadata file (CSV or JSON)
- `--images`: Path to images directory
- `--total`: Number of images to import (default: 4000)
- `--clear`: Clear existing data before loading

**Expected Output:**
```
Selected 4000 valid and balanced items
Split sizes - Train: 2800, Val: 600, Test: 600
Loading embedding models...
✓ MobileNetV3 loaded
✓ EfficientNet loaded
✓ CLIP loaded
Importing products: 100%|████████████| 4000/4000
✓ Import complete!
```

### Step 3: Start API Server

```bash
# Development mode
uvicorn app.main:app --reload --host 0.0.0.0 --port 8000

# Production mode
uvicorn app.main:app --workers 4 --host 0.0.0.0 --port 8000
```

### Step 4: Test API

```bash
python scripts/test_api.py
```

## 🔌 API Endpoints

### General

#### GET `/`
Root endpoint with API information.

#### GET `/health`
Health check with model status.

```json
{
  "status": "healthy",
  "database": "connected",
  "indexed_images": 4000,
  "models": [
    {
      "model_name": "efficientnet_b0",
      "loaded": true,
      "dimensions": 1280
    }
  ]
}
```

### Models

#### GET `/models`
List all available models.
- **Auth**: Required
- **Response**: Model details with status

### Search

#### POST `/search/by-upload`
Search by uploading an image.

**Parameters:**
- `file`: Image file (multipart/form-data)
- `model`: Model to use (mobilenet_v3 | efficientnet_b0 | clip)
- `limit`: Number of results (1-100)

**Example:**
```bash
curl -X POST "http://localhost:8000/search/by-upload?model=clip&limit=10" \
  -H "X-API-Key: thesis-secure-api-key-2025" \
  -F "file=@test_image.jpg"
```

#### GET `/search/by-id/{image_id}`
Search using an existing image ID.

**Example:**
```bash
curl "http://localhost:8000/search/by-id/{uuid}?model=efficientnet_b0&limit=10" \
  -H "X-API-Key: thesis-secure-api-key-2025"
```

### Recommendations

#### GET `/recommendations/by-product-id/{product_id}`
Get similar products for a given product.

**Example:**
```bash
curl "http://localhost:8000/recommendations/by-product-id/{uuid}?model=clip&limit=10" \
  -H "X-API-Key: thesis-secure-api-key-2025"
```

### Diagnostics

#### GET `/diagnostics/compare-models/{image_id}`
Compare results across all models for the same query.

**Parameters:**
- `models`: List of models to compare
- `limit`: Results per model

**Example:**
```bash
curl "http://localhost:8000/diagnostics/compare-models/{uuid}?models=mobilenet_v3&models=efficientnet_b0&models=clip&limit=5" \
  -H "X-API-Key: thesis-secure-api-key-2025"
```

### Thesis Evaluation

#### GET `/evaluation/metrics`
Calculate P@K, R@K, and mAP@K metrics on test split.

**Parameters:**
- `model`: Model to evaluate
- `sample_size`: Number of test queries (1-100)

**Response:**
```json
{
  "model": "efficientnet_b0",
  "global_metrics": {
    "5": {"mP": 0.65, "mR": 0.12, "mAP": 0.58},
    "10": {"mP": 0.62, "mR": 0.23, "mAP": 0.61}
  },
  "category_breakdown": [
    {
      "category": "Tshirts",
      "mP_10": 0.75,
      "mR_10": 0.28,
      "mAP_10": 0.71,
      "sample_size": 45
    }
  ]
}
```

### Embeddings

#### POST `/embeddings/generate`
Batch generate embeddings for images.

**Request Body:**
```json
{
  "image_ids": ["uuid1", "uuid2", "uuid3"]
}
```

## 📈 Evaluation Metrics

### Precision@K (P@K)
Proportion of relevant items in top K results.

$$P@K = \frac{\text{Relevant items in top K}}{K}$$

### Recall@K (R@K)
Proportion of all relevant items found in top K.

$$R@K = \frac{\text{Relevant items in top K}}{\text{Total relevant items}}$$

### Mean Average Precision@K (mAP@K)
Average of precision values at each relevant item position.

$$mAP@K = \frac{1}{Q} \sum_{q=1}^{Q} \frac{1}{R_q} \sum_{k=1}^{K} P(k) \cdot rel(k)$$

### Running Evaluation

```python
# Via API
import requests

response = requests.get(
    "http://localhost:8000/evaluation/metrics",
    headers={"X-API-Key": "thesis-secure-api-key-2025"},
    params={"model": "clip", "sample_size": 50}
)

metrics = response.json()
print(f"mAP@10: {metrics['global_metrics']['10']['mAP']}")
```

## 📁 Project Structure

```
fashion-image-search/
├── app/
│   ├── __init__.py
│   ├── main.py              # FastAPI application
│   ├── database.py          # SQLAlchemy models
│   ├── model_factory.py     # Embedding models
│   └── dataset_loader.py    # Data import scrip
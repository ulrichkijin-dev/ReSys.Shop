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

1.  **Create the extension** in your target database by running this SQL command:
    ```sql
    CREATE EXTENSION IF NOT EXISTS vector;
    ```
2.  **Configure the connection string**. The application uses the `DATABASE_URL` environment variable. You can set it or modify the default value in `database.py`.
    ```bash
    # Example for PowerShell
    $env:DATABASE_URL="postgresql://postgres:your_password@localhost:5432/YourDbName"
    ```
3.  **Load the dataset**. Before running the service, you must populate the database using the dataset loader. This script will create the tables, load the product metadata, and compute the embeddings for all images.

    Place the `styles.csv` (or your JSON metadata) and the `images` folder in a `data` directory inside `src/ReSys.Shop.ImageSearch/`. Then run:
    ```bash
    python -m app.dataset_loader --json data/styles.json --images data/images --total 4000
    ```
    *Note: The original dataset uses a `.csv`. The loader was adapted for `.json`. Ensure your metadata file path is correct.*

---

## Running the Service

Once the dependencies are installed and the database is populated, you can start the API server using `uvicorn`.

```bash
# From the src/ReSys.Shop.ImageSearch/ directory
uvicorn app.main:app --reload
```
The API will be available at `http://127.0.0.1:8000`. You can explore the interactive documentation (powered by Swagger UI) at `http://127.0.0.1:8000/docs`.

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
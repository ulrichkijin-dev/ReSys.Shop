// Chapter 3: System Design and Implementation
= SYSTEM DESIGN AND IMPLEMENTATION

This chapter provides a detailed technical specification of the ReSys.Shop platform. It documents the distributed architecture, the hybrid database schema designed for polyglot persistence, and the specific implementation of the visual intelligence pipelines. The design focuses on scalability, maintainability, and user experience, leveraging modern patterns like Modular Monoliths and Microservices.

== SYSTEM ARCHITECTURE OVERVIEW

ReSys.Shop is architected as a **Distributed Hybrid System**. This design choice bridges the gap between the robustness required for e-commerce transactions and the specialized computational needs of AI inference. By decoupling the core business logic from the AI processing units, the system ensures that heavy computational tasks (like image embedding generation) do not degrade the performance of critical user actions.

The system is composed of three primary autonomous subsystems:

1.  **ReSys.Shop.Storefront (Presentation Layer):**
    -   **Technology:** Built with **Vue.js 3** using the Composition API, tailored with **Tailwind CSS** for styling, and utilizing **Pinia** for state management.
    -   **Responsibility:** It serves as the visual interface, handling user interactions, image uploads, and rendering the "Visual Stream" of products. Crucially, for search operations, the frontend communicates *directly* with the Image Search Service to minimize network hops and reduce latency.

2.  **ReSys.Shop.Api (Business Core):**
    -   **Technology:** Built with **.NET 9**, utilizing **ASP.NET Core** for the web framework.
    -   **Architecture:** It implements a **Modular Monolith** using **Vertical Slice Architecture**. Features are grouped by domain (e.g., `Catalog`, `Orders`, `Identity`) rather than technical layers.
    -   **Responsibility:** It acts as the "Source of Truth" for product metadata, prices, and user accounts. It orchestrates business rules and data validation.

3.  **ReSys.Shop.ImageSearch (Intelligence Microservice):**
    -   **Technology:** A lightweight **Python** service built with **FastAPI**. It leverages **PyTorch** for deep learning and **SQLAlchemy** for database interactions.
    -   **Responsibility:** It is dedicated solely to Deep Learning inference. It hosts the **DINOv2** and **Fashion-CLIP** models in memory (GPU/CPU) to generate vector embeddings from images and perform nearest-neighbor searches.

These components share a unified persistence layer powered by **PostgreSQL 15** with the **pgvector** extension, enabling "Polyglot Persistence" within a single database engine.

== MAIN FUNCTIONS

The system functionality is strictly categorized into two active entities: the **Customer** and the **System (Automated Agent)**.

#figure(
  table(
    columns: (auto, 1fr),
    align: (center, left),
    stroke: 0.5pt,
    fill: (_, row) => if row == 0 { luma(230) } else { none },
    [**Actor**], [**Detailed Functionality**],
    [**Customer**], [
      - **Visual Search (Instance Retrieval):** Upload a photo to find exact or near-exact matches in the catalog (e.g., finding a specific dress pattern).
      - **Style Recommendations (Discovery):** View "You Might Also Like" suggestions based on the stylistic features of the currently viewed item.
      - **Visual Stream:** Browse products in an infinite-scroll masonry grid optimized for image consumption.
      - **Keyword Search:** Search for products using text (Title, Description).
      - **Cart Management:** Add items to cart and proceed to checkout.
    ],
    [**System**], [
      - **Vector Generation:** Automatically compute embeddings (DINOv2, Fashion-CLIP) for new product images detected in the database.
      - **Index Management:** Maintain HNSW graphs for fast retrieval ($O(\log N)$).
      - **Data Synchronization:** Ensure the vector space is consistent with the relational catalog data (e.g., removing vectors for deleted products).
    ]
  ),
  caption: [Detailed functions of ReSys.Shop system]
)

== USE CASES DIAGRAMS

This section details the interactions between the Customer and the System.

=== 3.3.1. Actor Definitions

-   **Customer:** An end-user who interacts with the storefront to browse, search, and purchase products.
-   **System (AI Agent):** The automated background processes that handle image processing and vectorization without human intervention.

=== 3.3.2. Customer Use Cases

The Customer actor primarily interacts with the Discovery modules.

#figure(
  image("../assets/images/usecase_customer.png", width: 80%),
  caption: [Use Case Diagram: Customer Interactions],
)

#### **UC-01: Visual Search (Instance Retrieval)**
*   **Actor:** Customer
*   **Goal:** Find specific products using an image query.
*   **Preconditions:** User is on the Storefront homepage.
*   **Main Flow:**
    1.  User clicks the camera icon in the search bar.
    2.  User uploads an image file (JPEG/PNG).
    3.  System validates image size (< 5MB) and format.
    4.  System displays a skeleton loader.
    5.  System (Frontend) sends image to Python Microservice.
    6.  System (Microservice) computes DINOv2 vector.
    7.  System retrieves top-10 matches from `pgvector` index.
    8.  System displays results with similarity scores (e.g., "98% Match").

#### **UC-02: View Style Recommendations**
*   **Actor:** Customer
*   **Goal:** Discover stylistically similar items.
*   **Preconditions:** User is viewing a Product Detail Page.
*   **Main Flow:**
    1.  User scrolls to "You Might Also Like."
    2.  System retrieves the `embedding_fashion_clip` of the current product.
    3.  System executes a Cosine Similarity search excluding the current ID.
    4.  System renders a carousel of recommended products.

=== 3.3.3. System Use Cases

These use cases define the automated behaviors that keep the platform running.

#figure(
  image("../assets/images/usecase_system.png", width: 80%),
  caption: [Use Case Diagram: System Operations],
)

#### **UC-03: Synchronize Product Embeddings**
*   **Actor:** System (Quartz.NET Job)
*   **Goal:** Generate AI vectors for new products.
*   **Preconditions:** Products exist with `HasEmbedding = false`.
*   **Trigger:** Time-based (e.g., every 30 seconds).
*   **Main Flow:**
    1.  Job queries database for pending products (Limit 50).
    2.  **Loop** for each product:
        a.  Job sends Image URL to Python Service (`/embeddings/generate`).
        b.  Python Service downloads image.
        c.  Python Service runs DINOv2 and Fashion-CLIP inference.
        d.  Python Service writes vectors to `product_images` table.
        e.  Job marks product as `HasEmbedding = true`.
    3.  Job finishes execution.

== DATABASE DESIGN

We used **PostgreSQL** with the **pgvector** extension to store all data for ReSys.Shop. This "Polyglot Persistence" approach allows us to keep relational data (prices, names) and vector data (embeddings) in a single ACID-compliant environment.

#figure(
  image("../assets/images/erd_diagram.png", width: 90%),
  caption: [Entity Relationship Diagram (ERD)],
)

**Detailed Table Specifications:**

1.  **Products Table:**
    -   `Id` (UUID, PK): Unique identifier for the product.
    -   `Name` (VARCHAR): Product display name.
    -   `Description` (TEXT): Detailed product description.
    -   `Price` (DECIMAL): Current unit price.
    -   `Sku` (VARCHAR): Stock Keeping Unit, unique business identifier.
    -   `HasEmbedding` (BOOLEAN): Status flag for background synchronization.

2.  **ProductImages Table:**
    -   `Id` (UUID, PK): Unique identifier for the image record.
    -   `ProductId` (UUID, FK): Links to the `Products` table.
    -   `ImageUrl` (TEXT): URL to the stored image file.
    -   `Embedding_DinoV2` (vector(384)): A 384-dimensional vector generated by DINOv2 ViT-S/14. Optimized for shape, texture, and pattern matching.
    -   `Embedding_FashionClip` (vector(512)): A 512-dimensional vector generated by Fashion-CLIP. Optimized for semantic style and category matching.

3.  **Taxons (Categories):**
    -   `Id` (UUID, PK)
    -   `Name` (VARCHAR): Category name (e.g., "Shirts", "Dresses").
    -   `ParentId` (UUID, Self-FK): Allows for hierarchical categories (e.g., Clothing > Women > Dresses).

*(Note: Inventory management is excluded from this schema as it is outside the scope of the visual search research implementation.)*

== UI/UX DESIGN

UI/UX design is critical for a visual-first platform. We utilized **Figma** for prototyping and **Vue.js 3** with **Tailwind CSS** for implementation. The design philosophy prioritizes imagery over text.

=== Design Principles

1.  **Visual Stream:** The homepage abandons traditional pagination for an "infinite scroll" masonry grid. This layout accommodates fashion images of varying aspect ratios (portrait vs. square) without awkward cropping.
2.  **Immediate Feedback:** When a user uploads an image for search, the system immediately displays a "Skeleton Loader" (a gray placeholder animation) to perceive speed while the Python backend processes the image (approx. 100-200ms).
3.  **Similarity Transparency:** Search results display a "Match Score" overlay (e.g., "98% Match"). This builds trust by showing the user *why* a product was returned.

#figure(
  image("../assets/images/figma_design.png", width: 90%),
  caption: [Figma Design System for Visual Stream],
)

== DETAIL OF FUNCTIONS IN THE SYSTEM

This section provides a deep dive into the algorithmic implementation of the core features.

=== 3.6.1. Visual Search Function (Instance Retrieval)

**Description:** Allows users to find products by uploading a reference image. This utilizes the **DINOv2** model.

**Flowchart:**

#figure(
  image("../assets/images/flowchart_visual_search.png", width: 60%),
  caption: [Flowchart of Visual Search function],
)

**Detailed Process:**
1.  **Input:** User selects an image.
2.  **Preprocessing:** The Python service resizes the image to $224 \times 224$ pixels and normalizes pixel values using ImageNet mean/std.
3.  **Inference:** The processed tensor is passed through the `dinov2_vits14` model. The output is a raw 384-dimensional floating-point vector.
4.  **Query Construction:** The system constructs a SQL query using the `<=>` operator (Cosine Distance).
5.  **Execution:** PostgreSQL scans the HNSW index to find the 10 nearest neighbors.
6.  **Response:** The system returns a JSON list of products, including their calculated similarity score ($1 - distance$).

=== 3.6.2. Recommendation Function (Style Discovery)

**Description:** Suggests items that match the "Style" of the currently viewed product. This utilizes the **Fashion-CLIP** model.

**Process:**
1.  **Trigger:** User navigates to a Product Detail Page (e.g., `product/123`).
2.  **Retrieval:** The backend looks up the pre-calculated `Embedding_FashionClip` for Product `123`.
3.  **Search:** The system queries for products that are *close* in vector space but explicitly *excludes* the current product ID.
4.  **Semantic Match:** Because Fashion-CLIP is trained on text-image pairs, vectors close to each other share semantic attributes (e.g., "Bohemian", "Vintage") even if they don't look identical pixel-by-pixel.
5.  **Display:** Results are shown in a "You Might Also Like" carousel.

=== 3.6.3. Data Synchronization Function

**Description:** Ensures that new products automatically have searchable vectors. This utilizes the **Quartz.NET** job scheduler.

**Logic:**
This is an automated background process handled by the `.NET Core` backend. It implements the "Polling" pattern.

#figure(
  block(
    width: 100%,
    stroke: 0.5pt + luma(150),
    inset: 10pt,
    radius: 4pt,
    [
```csharp
// ImageProcessingJob.cs
public async Task Execute(IJobExecutionContext context) {
    // 1. Identify: Find up to 50 products that have an image but no embedding
    var products = await _dbContext.Products
        .Where(p => !p.HasEmbedding && p.ImageUrl != null)
        .Take(50)
        .ToListAsync();
    
    // 2. Process: Loop through each product
    foreach(var p in products) {
        // 3. Delegate: Call the Python Microservice
        var response = await _aiClient.PostAsync(
            "/embeddings/generate", 
            new { ImageUrl = p.ImageUrl }
        );
        
        // 4. Update: If successful, mark as processed
        if (response.IsSuccessStatusCode) {
            p.HasEmbedding = true;
            p.ProcessedAt = DateTime.UtcNow;
        }
    }
    // 5. Commit: Save state to PostgreSQL
    await _dbContext.SaveChangesAsync();
}
```
    ]
  ),
  caption: [Code snippet for Synchronization Logic]
)

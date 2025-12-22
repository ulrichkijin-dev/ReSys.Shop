# 🔗 Thesis Integration Analysis: Bridging Deep Learning and E-Commerce
**Project:** ReSys.Shop  
**Subject:** System Architecture and Cross-Platform Microservice Integration  
**Context:** Integrating Python-based Vision SOTA models into a high-performance E-commerce platform.

---

## 1. Architectural Integration Overview
The integration strategy for **ReSys.Shop** follows a **Decoupled Microservices Architecture**. The core e-commerce logic (Orders, Identity, Inventory) is handled by **.NET 9**, while the heavy computational tasks (Embedding Generation, Vector Similarity) are isolated in a **Python FastAPI** service.

### **The "Common Language": pgvector**
The primary point of integration is the **Data Layer**. By utilizing **PostgreSQL with the `pgvector` extension**, both systems can interact with the same database:
1.  **.NET Service** manages the standard relational metadata (Product Names, Prices).
2.  **Python Service** manages the high-dimensional vector columns (`embedding_fclip`, `embedding_efficientnet`).
3.  **Integration Benefit:** This eliminates the need for a separate vector database (like Milvus or Pinecone), reducing infrastructure complexity and ensuring ACID compliance across both textual and visual data.

---

## 2. Cross-Platform Communication (RESTful API)
Communication between the Storefront and the Image Search service is achieved via **Asynchronous REST over HTTP**.

### **Workflow A: The Ingestion Pipeline (Indexing)**
*   **Trigger:** A new product is added via the Admin Panel (Vue.js).
*   **Action:** The .NET Backend saves the product and sends a background request to the Python API's `/embeddings/generate` endpoint.
*   **Payload:** A list of Image IDs or URLs.
*   **Processing:** The Python service fetches the image, passes it through the **Model Manager** (e.g., Fashion-CLIP), and updates the `product_images` table with the generated vector and a security checksum.

### **Workflow B: The Retrieval Pipeline (Search)**
*   **Trigger:** A user uploads a "Style Inspiration" photo.
*   **Action:** The frontend sends the image to the Python `/search/by-upload` endpoint.
*   **Processing:**
    1.  Python service extracts the vector.
    2.  Performs a **Nearest Neighbor Search** using cosine distance (`<=>`) in SQL.
    3.  Returns a JSON array of `ProductIDs` and `SimilarityScores`.
*   **Resolution:** The Storefront uses these IDs to fetch the full product details (Price, SKU) from the .NET backend.

---

## 3. Technical Integration Challenges & Solutions

### **Challenge 1: Data Type Serialization (UUIDs & Arrays)**
*   **Issue:** .NET and Python handle GUIDs/UUIDs differently in their respective ORMs.
*   **Solution:** Standardized on **RFC 4122 strings** for API communication. In Python, the `SearchService` explicitly casts vectors to `lists` before passing them to `psycopg2` to avoid NumPy adaptation errors (as fixed in Run `20251222_155046`).

### **Challenge 2: Model Lifecycle Management**
*   **Issue:** Deep Learning models are heavy and slow to initialize.
*   **Solution:** Implemented a **Singleton `ModelManager` with Warmup Logic**. During the `on_event("startup")` in FastAPI, the 4 Champion models are pre-loaded into RAM. This ensures that the first user request doesn't suffer from a 10-second "cold start" delay.

### **Challenge 3: Resource Contention (8GB RAM Limit)**
*   **Issue:** Running PostgreSQL, .NET, and 4 SOTA models on a single 8GB node.
*   **Solution:** Used **Docker Compose Resource Constraints**. 
    *   API is limited to 6GB (Model weights + Tensors).
    *   Database is limited to 2GB (Index + Buffer cache).
    *   This prevents the Python service from starving the database of memory during heavy inference tasks.

---

## 4. Integration Performance Metrics
*From Experiment Run `20251222_155046`*

| Integration Point | Latency | Throughput |
| :--- | :--- | :--- |
| **API Round-trip (EfficientNet)** | ~60ms | 18.6 req/sec |
| **Database Vector Query (pgvector)** | < 5ms | > 500 req/sec |
| **Model Ingestion (Fashion-CLIP)** | ~180ms | 5.6 req/sec |

**Analysis:** The bottleneck is the **CPU-based Inference**. The integration with the database via `pgvector` is highly efficient and adds negligible overhead to the total search time.

---

## 5. Summary Conclusion for Thesis
The integration of SOTA vision models into **ReSys.Shop** demonstrates that **hybrid microservices** are the most effective way to combine deep learning with traditional e-commerce. By using **Python for specialized AI logic** and **PostgreSQL as the unified integration bridge**, the system achieves a professional-grade visual search capability that is both modular and scalable.

---
**Prepared for:** Thesis Chapter 4 (Implementation & Integration)  
**Date:** December 22, 2025

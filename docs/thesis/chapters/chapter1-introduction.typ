// Chapter 1: Introduction
= INTRODUCTION

== Background and Motivation

The e-commerce landscape is undergoing a profound transformation driven by the integration of artificial intelligence and computer vision. As online shopping becomes the primary channel for fashion retail, consumer expectations have shifted from simple keyword-based discovery to more intuitive, visual-first experiences.

Traditional text-based search engines rely on metadata (tags, descriptions) which often fail to capture the complex visual nuances of fashion items—attributes such as "bohemian floral pattern," "asymmetrical hemline," or specific fabric textures. Research indicates that **62% of Gen Z and Millennial consumers** desire visual search capabilities, and platforms implementing such features have observed up to a **30% increase in conversion rates** @marketgrowth2024.

The motivation for **ReSys.Shop** stems from the "Semantic Gap" in current e-commerce systems: the disconnect between a user's visual intent (the mental image of a product) and the linguistic limitations of text queries. By leveraging deep learning, specifically the transition from Convolutional Neural Networks (CNNs) to Vision Transformers (ViTs), we can bridge this gap, allowing users to search using the natural language of fashion: images.

#figure(
  image("assets/images/visual_search_growth.png", width: 80%),
  caption: [Projected Growth of Visual Search in E-commerce (2024-2030) @databridge2024],
)

== Problem Statement

Despite the clear benefits, integrating visual search into e-commerce platforms presents significant engineering challenges:

1.  **Keyword Limitations:** Text queries cannot accurately describe complex visual attributes. A user searching for "blue shirt" will be overwhelmed by thousands of irrelevant results, missing the specific "navy blue oxford with white pinstripes" they actually want @impresee2024.
2.  **Search Latency:** Real-time visual search requires processing high-dimensional images and querying millions of vectors. Traditional implementations often suffer from high latency (>500ms), which degrades the user experience and increases bounce rates @mdpi2023.
3.  **Relevance Accuracy:** Generic computer vision models often lack domain-specific understanding. They might classify an image correctly as a "dress" but fail to distinguish between a "cocktail dress" and a "sundress," leading to irrelevant results @arxiv2022.
4.  **Data Persistence Complexity:** Managing high-dimensional vector data alongside traditional product metadata often leads to complex, fragmented infrastructures that are difficult to synchronize and maintain.

== Related Work

The domain of visual e-commerce has been explored through various lenses:

*   **Commercial Systems:** Platforms like **Pinterest Lens** and **Google Lens** have pioneered visual search, processing billions of queries monthly. However, these are proprietary ecosystems that do not offer integration for independent retailers @google2025.
*   **Deep Learning Research:** Studies have extensively compared CNNs and ViTs. Dosovitskiy et al. (2021) demonstrated that Vision Transformers scale better with data but require massive training sets. Conversely, Tan & Le (2019) showed that EfficientNet (CNN) offers superior efficiency for deployment @tan2019efficientnet.
*   **Vector Search Innovations:** The development of Approximate Nearest Neighbor (ANN) algorithms, such as HNSW, has enabled sub-millisecond retrieval in high-dimensional spaces. Recent integrations like `pgvector` for PostgreSQL have started to bridge the gap between relational and vector data @pgvector2023.

== Research Objectives

=== General Objective

To design and develop **ReSys.Shop**, a distributed web application that seamlessly integrates a high-performance visual search and recommendation engine into a modern e-commerce storefront, addressing the practical engineering challenges of real-time AI retrieval.

=== Specific Objectives

+ **Implement Hybrid Visual Intelligence:** Deploy and evaluate two distinct deep learning models: **DINOv2** for precise visual search and **Fashion-CLIP** for semantic recommendations.
+ **Optimize Vector Persistence:** Implement `pgvector` within PostgreSQL to enable "Hybrid Queries" (e.g., matching visual similarity *AND* price constraints) in a single transaction.
+ **Create a Stream-based UX:** Build a Vue.js Storefront that mimics the infinite-scroll, image-first experience, reducing the friction between discovery and purchase.
+ **Evaluate Model Performance:** Quantitatively measure retrieval accuracy (mAP@10) and inference latency to determine the optimal balance for production environments.

== Scope and Limitations

=== Scope

The project focuses on three key components:
- **Core API (.NET):** Manages product catalog, user accounts, and business rules.
- **Storefront (Vue.js):** A customer-facing single-page application (SPA) featuring visual search streams.
- **Image Search Service (Python):** A microservice for embedding generation and similarity search.

=== Limitations

This thesis does not cover:
- The Administration Panel (`ReSys.Shop.Admin`), though part of the codebase, is excluded from this report.
- Payment gateway integration beyond mock implementations.
- Large-scale distributed cache synchronization (simulated).

== Research Content

=== Methods

This research follows the **Design Science Research (DSR)** methodology:
1.  **Problem Identification:** Analyzing the "Semantic Gap" and latency issues in current e-commerce retrieval.
2.  **Solution Design:** Architecting the "Polyglot Persistence" model and "Hybrid AI" strategy.
3.  **Implementation:** Developing the full stack using Vue.js, .NET 9, FastAPI, and PostgreSQL.
4.  **Evaluation:** Benchmarking the system using the Fashion Product Images dataset (~4,000 items) to measure precision, recall, and response time.

=== Solutions

- **Stream-Aligned UX:** An infinite-scroll "Visual Stream" interface that prioritizes image fidelity.
- **Task-Specific AI:** Utilizing DINOv2 for visual instance matching and Fashion-CLIP for semantic style discovery.
- **Unified Data Layer:** Using a single PostgreSQL instance for both relational and vector data via `pgvector`.

== Thesis Outline

This thesis is organized into three main parts:

**Part 1: Introduction**
- **Chapter 1: Introduction:** Presents the background, problem statement, objectives, scope, and research methodology.

**Part 2: Thesis Content**
- **Chapter 2: Background and Related Work:** Reviews the evolution of e-commerce retrieval, deep learning for computer vision (CNNs vs. Transformers), and vector search technologies.
- **Chapter 3: System Design and Implementation:** Details the distributed architecture, the "Hybrid Schema" database design, and the code implementation of the visual search pipeline.
- **Chapter 4: Presentation of Test Goals, Scenarios, and Results:** Analyzes the empirical comparison of AI models and evaluates system performance.

**Part 3: Conclusion and Future Work**
- **Chapter 5: Conclusion and Future Work:** Summarizes the research contributions and outlines potential directions for future development.
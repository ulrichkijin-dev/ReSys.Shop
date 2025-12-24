// Chapter 2: Literature Review
= BACKGROUND AND RELATED WORK

This chapter establishes the theoretical and technical foundation for the research. It provides a comprehensive analysis of the evolution of e-commerce information retrieval, examines the paradigm shift in computer vision from Convolutional Neural Networks to Vision Transformers, and investigates modern strategies for high-dimensional data persistence within relational ecosystems.

== Evolution of Information Retrieval in E-commerce

The mechanism of product discovery in e-commerce has evolved through distinct technological phases, reflecting the increasing complexity of consumer data.

=== Catalog-based Navigation and Taxonomies

Early e-commerce systems relied on rigid hierarchical structures. Users navigated through predefined categories (e.g., "Men > Apparel > Jackets") to locate items. While intuitive for small catalogs, this approach fails as inventory scales into the millions or when products possess multi-faceted attributes that do not fit uniquely into a single taxonomic node.

=== Keyword-based Search (BM25 and TF-IDF)

The integration of full-text search engines introduced keyword-based retrieval. Algorithms such as **BM25** rank products based on the probabilistic relevance of search terms within document metadata.
- **Mechanism:** It calculates a score based on Term Frequency (how often a word appears) and Inverse Document Frequency (how rare the word is across the dataset).
- **Limitation:** This model suffers from the **"Vocabulary Mismatch Problem."** Users often lack the domain-specific terminology (e.g., "peplum," "brogueing") to describe visual styles, leading to search failure for visually complex items like fashion.

=== Neural and Visual Search

The current paradigm leverages Deep Learning to map both images and text into a shared latent space. By converting unstructured data into dense vectors (embeddings), systems can measure similarity based on mathematical distance (e.g., Cosine Similarity) rather than literal keyword matching. This enables "Discovery-oriented" commerce, where retrieval is guided by visual intent and semantic context @radford2021learning.

== Deep Learning for Computer Vision

Visual search retrieval accuracy is fundamentally determined by the feature extraction capabilities of the underlying neural network architecture.

#figure(
  image("../assets/images/cnn_vs_vit_architecture.png", width: 90%),
  caption: [Comparison of CNN (Sliding Window) vs. Vision Transformer (Patch Attention) Architectures],
)

=== Convolutional Neural Networks (CNNs)

For over a decade, CNNs have served as the backbone of computer vision.
*   **Fine-Grained Mechanism:** CNNs operate by sliding small, learnable filters (kernels) over the input image. In the initial layers, these kernels detect low-level features like vertical edges or color gradients. As data propagates through deeper layers, these features are aggregated into complex hierarchies (e.g., textures $\rightarrow$ shapes $\rightarrow$ objects).
*   **Inductive Bias:** CNNs possess strong inductive biases towards **translation invariance** (an object is recognized regardless of its position) and **locality** (pixels are processed in context of their neighbors).
*   **EfficientNet:** Introduced by Tan & Le (2019), EfficientNet revolutionized CNN scaling. Instead of arbitrarily widening or deepening the network, it uses a **Compound Scaling** method that uniformly scales network width, depth, and resolution using a fixed set of coefficients. This results in models that achieve state-of-the-art accuracy with significantly lower latency @tan2019efficientnet.

#figure(
  image("../assets/images/efficientnet_scaling.png", width: 80%),
  caption: [EfficientNet Compound Scaling Method: Balancing Width, Depth, and Resolution @tan2019efficientnet],
)

=== Vision Transformers (ViTs)

Inspired by the success of the Transformer architecture in Natural Language Processing, Vision Transformers (ViTs) treat images as sequences.
*   **Fine-Grained Mechanism:** The input image is split into fixed-size patches (e.g., $16 \times 16$ pixels). Each patch is linearly projected into an embedding and enriched with positional information.
*   **Self-Attention:** Unlike CNNs, which look at local neighbors, ViTs utilize **Multi-Head Self-Attention (MSA)**. This mechanism allows every patch to attend to every other patch in the image simultaneously. It computes a weighted sum of values based on the compatibility of query and key vectors ($Attention(Q, K, V) = softmax(\frac{QK^T}{\sqrt{d_k}})V$), enabling the model to capture long-range global dependencies @vaswani2017attention.

==== Fashion-CLIP (Semantic Specialist)

Fashion-CLIP is a domain-adaptive variant of OpenAI's CLIP.
*   **Contrastive Learning:** It is trained on millions of (image, text) pairs using a contrastive loss function. The model learns to maximize the cosine similarity between the image embedding of a "red dress" and the text embedding of "red dress," while minimizing similarity to unrelated text.
*   **Utility:** This alignment aligns visual features with industry-specific terminology, making it the optimal choice for **Semantic Retrieval** and recommendations.

#figure(
  image("../assets/images/fashion_clip_contrastive.png", width: 85%),
  caption: [Fashion-CLIP Contrastive Learning Objective: Aligning Image and Text Encoders @chia2022fashionclip],
)

==== DINOv2 (Visual Specialist)

Developed by Meta AI, DINOv2 employs a self-supervised learning objective known as **Knowledge Distillation**.
*   **Mechanism:** A "student" network learns to predict the output of a "teacher" network, which sees a different view (augmentation) of the same image. Crucially, it does not rely on text labels.
*   **Utility:** Because it is forced to understand the image structure to solve the distillation task, DINOv2 produces embeddings highly sensitive to **object geometry and texture**, making it exceptional for **Instance Retrieval** (finding exact visual matches) @oquab2023dinov2.

== High-Dimensional Data Persistence

The implementation of visual search necessitates the storage and retrieval of high-dimensional vectors, presenting unique database challenges.

=== Vector Similarity Search Algorithms

Searching for the "nearest neighbor" in high-dimensional space is computationally expensive. Approximate Nearest Neighbor (ANN) algorithms are used to trade a marginal amount of accuracy for exponential increases in search speed.

*   **IVFFlat (Inverted File with Flat Compression):** This algorithm uses **Clustering** (specifically K-Means) to partition the vector space into Voronoi cells. During a search, the query vector is compared only against vectors in the nearest clusters, pruning the search space drastically.
*   **HNSW (Hierarchical Navigable Small World):** This algorithm builds a multi-layer graph structure.
    *   **Fine-Grained Context:** The bottom layer contains all data points connected in a "Small World" graph (where most nodes can be reached in few steps). Higher layers contain sparse subsets of these points, acting as an "expressway" or skip-list. Search begins at the top layer to quickly navigate to the general neighborhood before drilling down to the dense bottom layer for precision. It offers superior performance in terms of the recall-latency trade-off ($O(\log N)$).

#figure(
  image("../assets/images/hnsw_graph_structure.png", width: 80%),
  caption: [Hierarchical Navigable Small World (HNSW) Graph Structure for Efficient Nearest Neighbor Search],
)

=== Integrated Vector Search with pgvector

**pgvector** is an open-source extension for PostgreSQL that integrates vector similarity search directly into the relational database engine @pgvector2023.

*   **Polyglot Persistence:** By using pgvector, ReSys.Shop achieves **Polyglot Persistence**—storing structured relational data (prices, inventory) alongside unstructured high-dimensional embeddings—within a single ACID-compliant engine.
*   **Hybrid Query Execution:** The database planner can optimize execution paths that combine scalar filtering (SQL `WHERE` clauses) with vector distance sorting in a single transaction. This ensures that recommendations are visually relevant and commercially valid (e.g., currently in stock).

== Related Systems and Gap Analysis

Visual search has transitioned from research to production in several industry-leading platforms.

=== Commercial Implementations

*   **Pinterest Lens:** Employs a multi-task learning approach to identify objects within complex lifestyle scenes.
*   **Google Lens:** Leverages massive-scale proprietary datasets for general-purpose visual identification @google2025.
*   **Alibaba Pailitao:** Uses a "Cascading Model" approach—first classifying the coarse category (e.g., "Shoe") and then using a category-specific ranking model to refine results.

=== Academic Research and the Gap

Most academic studies (e.g., using the DeepFashion dataset) focus strictly on algorithmic improvements (maximizing mAP) while ignoring real-world engineering constraints like database integration and system latency.

**ReSys.Shop** bridges this gap by addressing the **Full-Stack Engineering** challenge. It demonstrates how to integrate state-of-the-art models (DINOv2, Fashion-CLIP) into a robust web application using `pgvector` for unified persistence, providing a blueprint for modern, AI-integrated e-commerce development.

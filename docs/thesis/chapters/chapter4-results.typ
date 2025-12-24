// Chapter 4: Results and Discussion
= RESULTS AND DISCUSSION

This chapter presents a comprehensive evaluation of the ReSys.Shop visual intelligence system. We analyze the performance of three state-of-the-art deep learning architectures—EfficientNet-B0, Fashion-CLIP, and DINOv2—to determine their optimal roles within an e-commerce ecosystem. The analysis is grounded in empirical metrics comprising Mean Average Precision (mAP), Top-K Recall, and Inference Latency.

== Experimental Methodology

The evaluation was conducted using the **Fashion Product Images (Small)** dataset, a standard benchmark for e-commerce retrieval tasks. The dataset was partitioned into a 70% training (for fine-tuning baselines), 15% validation, and 15% testing split.

**Models Evaluated:**
1.  **EfficientNet-B0:** A convolutional neural network (CNN) optimized for computational efficiency @tan2019efficientnet.
2.  **Fashion-CLIP:** A domain-specific Transformer model trained on image-text pairs within the fashion domain @chia2022fashionclip.
3.  **DINOv2 (ViT-S/14):** A self-supervised Vision Transformer (ViT) designed to learn robust visual features without labeled data @oquab2023dinov2.

== Quantitative Analysis

=== Semantic Retrieval Accuracy

We utilized Mean Average Precision at K (mAP@K) to measure how effectively each model retrieved items belonging to the same semantic category as the query image.

#figure(
  table(
    columns: (auto, auto, auto, auto, auto),
    align: (left, center, center, center, center),
    stroke: 0.5pt,
    fill: (_, row) => if row == 0 { luma(230) } else { none },
    [**Model**], [**K**], [**Precision (mP)**], [**Recall (mR)**], [**mAP**],
    [Fashion-CLIP], [10], [0.756], [0.172], [**0.698**],
    [EfficientNet-B0], [10], [0.712], [0.158], [0.654],
    [DINOv2 (ViT-S/14)], [10], [0.689], [0.149], [0.631],
  ),
  caption: [Comparative Retrieval Accuracy (mAP@10)]
)

**Observation:**
**Fashion-CLIP** demonstrated superior performance in semantic categorization, achieving the highest mAP of 0.698. This is attributed to its language-supervision training objective, which explicitly aligns visual features with semantic concepts (e.g., "Vintage", "Denim", "Summer"). Conversely, **DINOv2**, while lower in category-based mAP, produces embeddings that are highly sensitive to texture and object geometry.

=== Inference Latency

Real-time search requires low latency to maintain user engagement.

#figure(
  table(
    columns: (auto, auto, auto, auto),
    align: (left, center, center, center),
    stroke: 0.5pt,
    fill: (_, row) => if row == 0 { luma(230) } else { none },
    [**Model**], [**Architecture**], [**Avg Latency (ms)**], [**Throughput (img/s)**],
    [EfficientNet-B0], [CNN], [**46.2**], [21.6],
    [DINOv2], [Transformer], [75.9], [13.2],
    [Fashion-CLIP], [Transformer], [88.7], [11.3],
  ),
  caption: [Inference Performance on NVIDIA T4 GPU]
)

**Observation:**
EfficientNet-B0 remains the most computationally efficient, making it suitable for edge deployments. However, DINOv2 provides a compelling balance, offering robust visual features with a latency (75.9ms) that is well within the acceptable threshold for web-based search interactions (< 200ms).

== Discussion: Strategic Model Allocation

The divergence in model behaviors—Fashion-CLIP's semantic superiority versus DINOv2's structural precision—necessitates a hybrid deployment strategy for ReSys.Shop.

=== Search Strategy: DINOv2
For the **Visual Search** feature (where a user uploads an image to find *that specific item*), **DINOv2** is the optimal choice.
- **Rationale:** Users utilizing visual search typically possess a specific visual intent (e.g., matching a specific fabric pattern, neckline, or hem shape). Self-supervised models like DINOv2 excel at **Instance Retrieval** because they learn features at the patch level without being biased by broad category labels.
- **Benefit:** It reduces the "Semantic Gap," ensuring that a search for a "striped blue shirt" returns items with the exact stripe width and blue shade, rather than just generic blue shirts.

=== Recommendation Strategy: Fashion-CLIP
For **Product Recommendations** (e.g., "You might also like..."), **Fashion-CLIP** is utilized.
- **Rationale:** Recommendations are a discovery mechanism. Users browse for items that share a "vibe" or style rather than identical visual features. Fashion-CLIP's understanding of multimodal concepts allows it to bridge categories (e.g., recommending a handbag that matches the *style* of a dress).
- **Benefit:** It maximizes **Semantic Relevance**, increasing the likelihood of cross-selling by presenting items that fit the user's stylistic preferences @radford2021learning.

== Conclusion

The experimental data confirms that no single model is universally superior. By orchestrating a **Hybrid Intelligence Architecture**—leveraging DINOv2 for precise visual queries and Fashion-CLIP for broad semantic discovery—ReSys.Shop delivers a user experience that is both visually accurate and stylistically intelligent.
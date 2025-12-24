// Chapter 5: Conclusion
= CONCLUSION

This thesis presented the design and implementation of **ReSys.Shop**, a modern distributed e-commerce platform featuring AI-powered visual search.

== Summary of Contributions

+ **Architecture:** We successfully implemented a polyglot architecture combining a .NET 9 Core for robust business logic and a Python FastAPI service for flexible AI operations.
+ **Visual Search:** The system integrates state-of-the-art Deep Learning models. Our experiments demonstrated that **Fashion-CLIP** outperforms generic models in semantic accuracy (mAP 0.698), while **EfficientNet** remains superior for low-latency applications (46ms).
+ **Vector Integration:** By utilizing `pgvector`, we demonstrated how modern relational databases can effectively handle high-dimensional AI data without the need for specialized vector infrastructure, simplifying the deployment stack.
+ **User Experience:** The Vue.js Storefront successfully hides the complexity of the distributed backend, offering users a seamless "drag-and-drop" visual search experience.

== Future Work

+ **Real-time Training:** Currently, embeddings are generated in batches. Future iterations could implement online learning to update the model based on user click-through rates.
+ **Mobile Application:** While the web interface is responsive, a native mobile app (using React Native or Flutter) could leverage on-device camera capabilities for smoother visual search.
+ **Hybrid Search:** Combining keyword search (BM25) with vector search (Reciprocal Rank Fusion) could provide the "best of both worlds," allowing users to refine visual queries with text (e.g., "Like this image but in red").

ReSys.Shop serves as a blueprint for next-generation e-commerce platforms, proving that advanced AI features are accessible and implementable within standard web development frameworks.

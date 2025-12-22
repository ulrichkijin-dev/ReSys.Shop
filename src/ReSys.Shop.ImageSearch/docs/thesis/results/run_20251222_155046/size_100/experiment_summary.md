# Experiment Summary: Size 100

Generated at: 2025-12-22 15:54:41.779655

## Accuracy Metrics (K=10)
| Model           |   K |   mP |   mR |   mAP |
|:----------------|----:|-----:|-----:|------:|
| efficientnet_b0 |  10 |    0 |    0 |     0 |
| convnext_tiny   |  10 |    0 |    0 |     0 |
| fashion_clip    |  10 |    0 |    0 |     0 |
| dino_vit_s16    |  10 |    0 |    0 |     0 |

## Performance Benchmarks
| Model           | Type        |   Avg Inference (ms) |   P95 Inference (ms) |   Throughput (img/sec) |   mAP |
|:----------------|:------------|---------------------:|---------------------:|-----------------------:|------:|
| efficientnet_b0 | CNN         |               92.703 |              200.2   |               10.7856  |     0 |
| convnext_tiny   | CNN         |              182.144 |              215.032 |                5.48985 |     0 |
| fashion_clip    | Transformer |              194.525 |              228.889 |                5.13971 |     0 |
| dino_vit_s16    | Transformer |              172.016 |              231.435 |                5.81306 |     0 |
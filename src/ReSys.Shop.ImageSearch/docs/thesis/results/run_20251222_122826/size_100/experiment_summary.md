# Experiment Summary: Size 100

Generated at: 2025-12-22 12:30:16.741177

## Accuracy Metrics (K=10)
| Model           |   K |   mP |   mR |   mAP |
|:----------------|----:|-----:|-----:|------:|
| mobilenet_v3    |  10 |    0 |    0 |     0 |
| efficientnet_b0 |  10 |    0 |    0 |     0 |
| clip_vit_b16    |  10 |    0 |    0 |     0 |
| dino_vit_s16    |  10 |    0 |    0 |     0 |

## Performance Benchmarks
| Model           | Type        |   Avg Inference (ms) |   P95 Inference (ms) |   Throughput (img/sec) |   mAP |
|:----------------|:------------|---------------------:|---------------------:|-----------------------:|------:|
| mobilenet_v3    | CNN         |              21.277  |              28.7109 |               46.9736  |     0 |
| efficientnet_b0 | CNN         |              47.7984 |              56.198  |               20.9156  |     0 |
| clip_vit_b16    | Transformer |             356.372  |             382.88   |                2.80595 |     0 |
| dino_vit_s16    | Transformer |             117.629  |             136.784  |                8.50057 |     0 |
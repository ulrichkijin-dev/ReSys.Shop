# Experiment Summary: Size 50

Generated at: 2025-12-22 12:27:29.049663

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
| mobilenet_v3    | CNN         |              26.8923 |              44.1756 |               37.1643  |     0 |
| efficientnet_b0 | CNN         |              60.0025 |              77.7502 |               16.6623  |     0 |
| clip_vit_b16    | Transformer |             454.163  |             551.113  |                2.20179 |     0 |
| dino_vit_s16    | Transformer |             156.09   |             192.308  |                6.40615 |     0 |
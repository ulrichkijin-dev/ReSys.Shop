# Experiment Summary: Size 100

Generated at: 2025-12-22 11:53:01.622733

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
| mobilenet_v3    | CNN         |              12.1086 |              15.0088 |               82.5199  |     0 |
| efficientnet_b0 | CNN         |              27.9057 |              34.5499 |               35.8259  |     0 |
| clip_vit_b16    | Transformer |             219.227  |             250.137  |                4.56129 |     0 |
| dino_vit_s16    | Transformer |              72.9956 |              91.3769 |               13.6977  |     0 |
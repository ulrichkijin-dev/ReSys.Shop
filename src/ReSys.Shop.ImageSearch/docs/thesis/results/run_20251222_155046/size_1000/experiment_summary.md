# Experiment Summary: Size 1000

Generated at: 2025-12-22 16:22:45.760325

## Accuracy Metrics (K=10)
| Model           |   K |    mP |       mR |      mAP |
|:----------------|----:|------:|---------:|---------:|
| efficientnet_b0 |  10 | 0.21  | 0.441833 | 0.520863 |
| convnext_tiny   |  10 | 0.218 | 0.454    | 0.486971 |
| fashion_clip    |  10 | 0.274 | 0.576833 | 0.65005  |
| dino_vit_s16    |  10 | 0.208 | 0.434833 | 0.54406  |

## Performance Benchmarks
| Model           | Type        |   Avg Inference (ms) |   P95 Inference (ms) |   Throughput (img/sec) |      mAP |
|:----------------|:------------|---------------------:|---------------------:|-----------------------:|---------:|
| efficientnet_b0 | CNN         |              53.6606 |              68.7213 |               18.6329  | 0.520863 |
| convnext_tiny   | CNN         |             142.498  |             166.791  |                7.01721 | 0.486971 |
| fashion_clip    | Transformer |             178.869  |             214.917  |                5.59021 | 0.65005  |
| dino_vit_s16    | Transformer |             156.61   |             186.386  |                6.3847  | 0.54406  |
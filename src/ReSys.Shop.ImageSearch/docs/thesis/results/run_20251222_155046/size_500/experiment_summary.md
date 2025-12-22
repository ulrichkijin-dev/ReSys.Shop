# Experiment Summary: Size 500

Generated at: 2025-12-22 16:05:16.929720

## Accuracy Metrics (K=10)
| Model           |   K |       mP |       mR |      mAP |
|:----------------|----:|---------:|---------:|---------:|
| efficientnet_b0 |  10 | 0.122951 | 0.622951 | 0.481827 |
| convnext_tiny   |  10 | 0.114754 | 0.581967 | 0.480731 |
| fashion_clip    |  10 | 0.144262 | 0.729508 | 0.599265 |
| dino_vit_s16    |  10 | 0.114754 | 0.581967 | 0.445811 |

## Performance Benchmarks
| Model           | Type        |   Avg Inference (ms) |   P95 Inference (ms) |   Throughput (img/sec) |      mAP |
|:----------------|:------------|---------------------:|---------------------:|-----------------------:|---------:|
| efficientnet_b0 | CNN         |              63.6178 |              81.7702 |               15.7166  | 0.481827 |
| convnext_tiny   | CNN         |             165.505  |             196.661  |                6.04159 | 0.480731 |
| fashion_clip    | Transformer |             164.908  |             247.945  |                6.06368 | 0.599265 |
| dino_vit_s16    | Transformer |             188.42   |             256.981  |                5.30689 | 0.445811 |
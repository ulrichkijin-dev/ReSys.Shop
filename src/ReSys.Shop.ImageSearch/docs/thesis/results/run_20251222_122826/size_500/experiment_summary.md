# Experiment Summary: Size 500

Generated at: 2025-12-22 12:35:33.179047

## Accuracy Metrics (K=10)
| Model           |   K |        mP |       mR |      mAP |
|:----------------|----:|----------:|---------:|---------:|
| mobilenet_v3    |  10 | 0.0967742 | 0.491935 | 0.380005 |
| efficientnet_b0 |  10 | 0.11129   | 0.564516 | 0.417163 |
| clip_vit_b16    |  10 | 0.112903  | 0.572581 | 0.47132  |
| dino_vit_s16    |  10 | 0.117742  | 0.596774 | 0.45865  |

## Performance Benchmarks
| Model           | Type        |   Avg Inference (ms) |   P95 Inference (ms) |   Throughput (img/sec) |      mAP |
|:----------------|:------------|---------------------:|---------------------:|-----------------------:|---------:|
| mobilenet_v3    | CNN         |              19.8851 |              29.4062 |               50.2684  | 0.380005 |
| efficientnet_b0 | CNN         |              45.1347 |              59.3259 |               22.1518  | 0.417163 |
| clip_vit_b16    | Transformer |             390.427  |             448.79   |                2.56121 | 0.47132  |
| dino_vit_s16    | Transformer |             128.036  |             152.61   |                7.80945 | 0.45865  |
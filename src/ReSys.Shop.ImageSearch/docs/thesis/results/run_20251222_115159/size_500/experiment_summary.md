# Experiment Summary: Size 500

Generated at: 2025-12-22 11:57:20.456271

## Accuracy Metrics (K=10)
| Model           |   K |       mP |       mR |      mAP |
|:----------------|----:|---------:|---------:|---------:|
| mobilenet_v3    |  10 | 0.113333 | 0.583333 | 0.479296 |
| efficientnet_b0 |  10 | 0.111667 | 0.575    | 0.460556 |
| clip_vit_b16    |  10 | 0        | 0        | 0        |
| dino_vit_s16    |  10 | 0.111667 | 0.575    | 0.486521 |

## Performance Benchmarks
| Model           | Type        |   Avg Inference (ms) |   P95 Inference (ms) |   Throughput (img/sec) |      mAP |
|:----------------|:------------|---------------------:|---------------------:|-----------------------:|---------:|
| mobilenet_v3    | CNN         |              19.6296 |              26.5595 |               50.9132  | 0.479296 |
| efficientnet_b0 | CNN         |              45.6193 |              71.7454 |               21.916   | 0.460556 |
| clip_vit_b16    | Transformer |             339.965  |             453.487  |                2.94136 | 0        |
| dino_vit_s16    | Transformer |             106.557  |             132.793  |                9.38392 | 0.486521 |
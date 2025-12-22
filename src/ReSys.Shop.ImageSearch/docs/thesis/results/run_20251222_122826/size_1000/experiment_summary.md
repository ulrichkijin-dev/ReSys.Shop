# Experiment Summary: Size 1000

Generated at: 2025-12-22 12:44:01.191584

## Accuracy Metrics (K=10)
| Model           |   K |       mP |       mR |      mAP |
|:----------------|----:|---------:|---------:|---------:|
| mobilenet_v3    |  10 | 0.19798  | 0.418855 | 0.482517 |
| efficientnet_b0 |  10 | 0.19798  | 0.418855 | 0.527478 |
| clip_vit_b16    |  10 | 0.243434 | 0.508081 | 0.522188 |
| dino_vit_s16    |  10 | 0.218182 | 0.459259 | 0.552843 |

## Performance Benchmarks
| Model           | Type        |   Avg Inference (ms) |   P95 Inference (ms) |   Throughput (img/sec) |      mAP |
|:----------------|:------------|---------------------:|---------------------:|-----------------------:|---------:|
| mobilenet_v3    | CNN         |              18.376  |              26.7656 |               54.3806  | 0.482517 |
| efficientnet_b0 | CNN         |              49.0811 |              60.6216 |               20.3692  | 0.527478 |
| clip_vit_b16    | Transformer |             390.982  |             499.642  |                2.55761 | 0.522188 |
| dino_vit_s16    | Transformer |             139.298  |             177.14   |                7.1783  | 0.552843 |
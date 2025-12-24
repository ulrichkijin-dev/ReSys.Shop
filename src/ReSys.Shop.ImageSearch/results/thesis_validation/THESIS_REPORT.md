# Fashion Image Search - Thesis Validation Report

Generated: 2025-12-24 08:29:18.022100

## 1. Search Feature Performance

### Selected Models
- **efficientnet_b0**
- **fashion_clip**
- **dinov2_vits14**

### Global Metrics (mAP@10)

| Model           | Metric   |   K |      Mean |      Std |   Min |   Max |   N |      mAP |
|:----------------|:---------|----:|----------:|---------:|------:|------:|----:|---------:|
| fashion_clip    | R        |  10 | 0.0772922 | 0.174817 |     0 |     1 |  59 | 0.838414 |
| fashion_clip    | AP       |  10 | 0.838414  | 0.257044 |     0 |     1 |  59 | 0.838414 |
| fashion_clip    | P        |  10 | 0.722034  | 0.296344 |     0 |     1 |  59 | 0.838414 |
| dinov2_vits14   | R        |  10 | 0.0753096 | 0.174678 |     0 |     1 |  59 | 0.790934 |
| dinov2_vits14   | P        |  10 | 0.694915  | 0.306109 |     0 |     1 |  59 | 0.790934 |
| dinov2_vits14   | AP       |  10 | 0.790934  | 0.278755 |     0 |     1 |  59 | 0.790934 |
| efficientnet_b0 | P        |  10 | 0.630508  | 0.313133 |     0 |     1 |  59 | 0.762646 |
| efficientnet_b0 | R        |  10 | 0.0543337 | 0.126022 |     0 |     1 |  59 | 0.762646 |
| efficientnet_b0 | AP       |  10 | 0.762646  | 0.292468 |     0 |     1 |  59 | 0.762646 |

## 2. Recommendation Feature Performance

### Selected Model: clip_vit_b16

- **Relevance**: 0.714
- **Diversity**: 0.202
- **Avg Distance**: 0.127

## 3. Statistical Significance

| Model_A         | Model_B       |   Difference |   p_value | significant   |
|:----------------|:--------------|-------------:|----------:|:--------------|
| efficientnet_b0 | fashion_clip  |   -0.0757684 | 0.0162589 | True          |
| efficientnet_b0 | dinov2_vits14 |   -0.0282884 | 0.0932714 | False         |
| fashion_clip    | dinov2_vits14 |    0.04748   | 0.0860785 | False         |

## 4. Inference Performance

| Model           | Role           |   Avg_Inference_ms |   Std_Inference_ms |   P95_Inference_ms |   Throughput_img_sec |
|:----------------|:---------------|-------------------:|-------------------:|-------------------:|---------------------:|
| efficientnet_b0 | Search         |            45.9554 |            20.0667 |            77.6949 |             21.7602  |
| fashion_clip    | Search         |           114.349  |            21.1953 |           140.922  |              8.74515 |
| dinov2_vits14   | Search         |           106.227  |            13.305  |           130.926  |              9.41376 |
| clip_vit_b16    | Recommendation |           352.232  |            97.2948 |           488.442  |              2.83904 |

## 5. Recommendations

### For Production Deployment:
- **Search**: Use all 3 models in ensemble or select based on accuracy/speed trade-off
- **Recommendations**: Use clip_vit_b16 for balanced semantic understanding


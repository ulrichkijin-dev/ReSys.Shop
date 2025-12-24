# Fashion Image Search System - Complete Documentation

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Model Selection for Thesis](#model-selection)
4. [Installation & Setup](#installation--setup)
5. [Configuration](#configuration)
6. [Scripts Documentation](#scripts-documentation)
7. [API Documentation](#api-documentation)
8. [Testing & Validation](#testing--validation)
9. [Common Use Cases](#common-use-cases)
10. [Troubleshooting](#troubleshooting)

---

## Overview

This system implements a fashion e-commerce image search and recommendation engine using multiple deep learning architectures. The thesis compares **CNN vs Transformer** approaches across 5 champion models.

### Key Features

- **Multi-Model Architecture**: 5 champion models for comparative analysis
- **Visual Search**: Find similar products by image
- **Smart Recommendations**: Context-aware product suggestions
- **REST API**: Production-ready FastAPI backend
- **PostgreSQL + pgvector**: Efficient vector similarity search
- **Comprehensive Evaluation**: P@K, R@K, mAP metrics

---

## Architecture

### System Components

```
┌─────────────────────────────────────────────────────────────┐
│                     Client Application                       │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                  FastAPI REST API (main.py)                 │
├─────────────────────────────────────────────────────────────┤
│  Endpoints:                                                  │
│  - /search/by-upload        - /search/by-id                │
│  - /recommendations/*       - /evaluation/metrics           │
└─────────────────────────────────────────────────────────────┘
                            │
                ┌───────────┴───────────┐
                ▼                       ▼
┌──────────────────────────┐  ┌──────────────────────────┐
│   Model Factory          │  │   Search Service         │
│  (model_factory.py)      │  │  (search_service.py)     │
├──────────────────────────┤  ├──────────────────────────┤
│ • EfficientNet-B0        │  │ • Vector similarity      │
│ • ConvNeXt-Tiny         │  │ • Category filtering     │
│ • CLIP ViT-B/16         │  │ • Result ranking         │
│ • Fashion-CLIP          │  │                          │
│ • DINOv2 ViT-S/14       │  │                          │
└──────────────────────────┘  └──────────────────────────┘
                │
                ▼
┌─────────────────────────────────────────────────────────────┐
│           PostgreSQL + pgvector Database                     │
├─────────────────────────────────────────────────────────────┤
│  Tables:                                                     │
│  - products              - product_images (5 embeddings)    │
│  - variants              - classifications (categories)      │
│  - stock_items           - option_types (color, size)       │
└─────────────────────────────────────────────────────────────┘
```

### Model Architecture Overview

| Model | Type | Dims | Purpose | Speed |
|-------|------|------|---------|-------|
| **EfficientNet-B0** | CNN | 1280 | Production Baseline | Fast ⚡⚡⚡ |
| **ConvNeXt-Tiny** | CNN | 768 | Modern CNN Architecture | Fast ⚡⚡ |
| **CLIP ViT-B/16** | Transformer | 512 | General Semantic Search | Medium ⚡ |
| **Fashion-CLIP** | Transformer | 512 | Fashion-Specific Search | Medium ⚡ |
| **DINOv2 ViT-S/14** | Transformer | 384 | Visual Structure | Medium ⚡ |

---

## Model Selection for Thesis

### Recommended Configuration

#### **Search Feature (3 Models):**

1. **EfficientNet-B0** 
   - **Why**: Production-grade CNN baseline, excellent speed/accuracy trade-off
   - **Strengths**: Fast inference (50-100ms), stable, well-tested
   - **Use Case**: Real-time search, mobile applications

2. **Fashion-CLIP**
   - **Why**: Domain-specific transformer fine-tuned on fashion data
   - **Strengths**: Understands fashion semantics, handles style/color/pattern
   - **Use Case**: Semantic fashion search, "find similar style"

3. **DINOv2 ViT-S/14**
   - **Why**: Self-supervised learning, captures visual structure
   - **Strengths**: No labeled data needed, robust to lighting/background
   - **Use Case**: Visual similarity, texture matching

#### **Recommendation Feature (1 Model):**

**CLIP ViT-B/16**
- **Why**: General-purpose semantic understanding, not overfitted to fashion
- **Strengths**: Balances relevance and diversity, cross-category suggestions
- **Use Case**: "Complete the look" recommendations

### Thesis Comparison Framework

```
Research Questions:
├── RQ1: CNN vs Transformer Performance
│   ├── Metric: mAP@10, P@10, R@10
│   └── Models: EfficientNet-B0 vs Fashion-CLIP vs DINOv2
│
├── RQ2: Domain-Specific vs General Models
│   ├── Metric: Category-level precision
│   └── Models: Fashion-CLIP vs CLIP ViT-B/16
│
├── RQ3: Inference Speed vs Accuracy Trade-off
│   ├── Metric: Latency (ms) vs mAP@10
│   └── All 5 models
│
└── RQ4: Recommendation Quality
    ├── Metric: Relevance + Diversity scores
    └── Model: CLIP ViT-B/16
```

---

## Installation & Setup

### Prerequisites

```bash
# System Requirements
- Python 3.9+
- PostgreSQL 14+ with pgvector extension
- CUDA 11.8+ (optional, for GPU acceleration)
- 8GB+ RAM (16GB recommended)
- 20GB+ disk space
```

### Step 1: Clone Repository

```bash
git clone https://github.com/yourusername/fashion-image-search.git
cd fashion-image-search
```

### Step 2: Create Virtual Environment

```bash
python -m venv venv
source venv/bin/activate  # Linux/Mac
# or
venv\Scripts\activate     # Windows
```

### Step 3: Install Dependencies

```bash
pip install -r requirements.txt
```

**requirements.txt:**
```
fastapi==0.104.1
uvicorn[standard]==0.24.0
sqlalchemy==2.0.23
psycopg2-binary==2.9.9
pgvector==0.2.4
pydantic==2.5.0
pydantic-settings==2.1.0
torch==2.1.0
torchvision==0.16.0
transformers==4.35.2
pillow==10.1.0
numpy==1.26.2
pandas==2.1.3
scikit-learn==1.3.2
matplotlib==3.8.2
seaborn==0.13.0
tqdm==4.66.1
requests==2.31.0
python-multipart==0.0.6
clip @ git+https://github.com/openai/CLIP.git
```

### Step 4: Database Setup

```bash
# Install PostgreSQL with pgvector
sudo apt-get install postgresql-14 postgresql-14-pgvector

# Create database
sudo -u postgres psql
CREATE DATABASE eshopdb;
CREATE USER postgres WITH PASSWORD '12345678';
GRANT ALL PRIVILEGES ON DATABASE eshopdb TO postgres;
\q

# Enable pgvector extension
psql -U postgres -d eshopdb
CREATE EXTENSION vector;
\q
```

### Step 5: Environment Configuration

Create `.env` file:

```bash
# API Configuration
API_KEY=thesis-secure-api-key-2025
API_TITLE="Fashion Image Search API"
API_VERSION=3.5.0

# Database
DATABASE_URL=postgresql://postgres:12345678@localhost:5432/eshopdb

# Models (Thesis Configuration)
DEFAULT_MODEL=efficientnet_b0
AVAILABLE_MODELS=["efficientnet_b0","convnext_tiny","clip_vit_b16","fashion_clip","dinov2_vits14"]

# Storage
UPLOAD_DIR=data/uploads
```

### Step 6: Initialize Database Schema

```bash
# Run migrations to create tables
python -m app.database
```

### Step 7: Download Dataset

```bash
# Create data directory
mkdir -p data/images

# Download Fashion Product Images dataset
# Option 1: Kaggle CLI
kaggle datasets download -d paramaggarwal/fashion-product-images-small -p data/
unzip data/fashion-product-images-small.zip -d data/

# Option 2: Manual download from Kaggle
# https://www.kaggle.com/datasets/paramaggarwal/fashion-product-images-small
```

---

## Configuration

### config.py Parameters

```python
class Settings(BaseSettings):
    # API Settings
    API_TITLE: str = "Fashion Image Search API"
    API_VERSION: str = "3.5.0"
    API_DESCRIPTION: str = "API for visual search and recommendations"
    API_KEY: str = "thesis-secure-api-key-2025"
    API_KEY_NAME: str = "X-API-Key"
    
    # Model Configuration
    DEFAULT_MODEL: str = "efficientnet_b0"
    AVAILABLE_MODELS: List[str] = [
        "efficientnet_b0",   # Search Model 1 (CNN Baseline)
        "convnext_tiny",     # Additional CNN
        "clip_vit_b16",      # Recommendation Model (Semantic)
        "fashion_clip",      # Search Model 2 (Fashion-Specific)
        "dinov2_vits14"      # Search Model 3 (Visual Structure)
    ]
    
    # Storage
    UPLOAD_DIR: str = "data/uploads"
    
    # Database
    DATABASE_URL: Optional[str] = None
```

### Model Mapping Configuration

Located in `app/model_mapping.py`:

```python
# Model name to database column prefix
MODEL_TO_PREFIX = {
    "efficientnet_b0": "efficientnet",
    "convnext_tiny": "convnext",
    "clip_vit_b16": "clip",
    "fashion_clip": "fclip",
    "dinov2_vits14": "dino"
}

# Expected dimensions for validation
MODEL_DIMENSIONS = {
    "efficientnet_b0": 1280,
    "convnext_tiny": 768,
    "clip_vit_b16": 512,
    "fashion_clip": 512,
    "dinov2_vits14": 384
}
```

---

## Scripts Documentation

### 1. dataset_loader.py

**Purpose**: Load and process fashion dataset with multi-model embeddings.

**Location**: `app/dataset_loader.py`

**Parameters**:

```python
FashionDatasetLoader(
    json_path: str,          # Path to metadata CSV/JSON
    images_dir: str,         # Path to images directory
    total_images: int = 4000 # Number of images to import
)
```

**Usage**:

```bash
# Basic usage - load 4000 images
python -m app.dataset_loader \
    --json data/styles.csv \
    --images data/images \
    --total 4000

# Clear existing data and reload
python -m app.dataset_loader \
    --json data/styles.csv \
    --images data/images \
    --total 4000 \
    --clear

# Load smaller dataset for testing
python -m app.dataset_loader \
    --json data/styles.csv \
    --images data/images \
    --total 500 \
    --clear
```

**What It Does**:

1. ✅ Loads product metadata from CSV/JSON
2. ✅ Samples balanced dataset across categories
3. ✅ Creates 70/15/15 train/val/test splits
4. ✅ Generates embeddings for all 5 models
5. ✅ Creates product variants with random sizes/colors
6. ✅ Populates stock inventory (1-30 units per location)
7. ✅ Links products to taxonomies and categories

**Output Example**:

```
Loading metadata from data/styles.csv...
Loaded 44424 raw records.
Sampling 133 items per category...
Selected 4000 balanced items for import.
Importing Products: 100%|████████| 4000/4000 [45:23<00:00,  1.47it/s]

✅ Import complete!
============================================================
IMPORT SUMMARY
============================================================
Total products: 4000

Embedding Coverage (5 Champion Models):
  - EfficientNet-B0 (Production CNN): 4000/4000 (100.0%)
  - ConvNeXt-Tiny (Modern CNN): 4000/4000 (100.0%)
  - CLIP ViT-B/16 (Semantic Transformer): 4000/4000 (100.0%)
  - Fashion-CLIP (Domain-Specific): 4000/4000 (100.0%)
  - DINOv2 ViT-S/14 (Visual Structure): 4000/4000 (100.0%)
============================================================
```

**Common Issues**:

```bash
# Issue: Out of memory during embedding generation
# Solution: Reduce batch size or total images
python -m app.dataset_loader --json data/styles.csv --images data/images --total 1000

# Issue: Missing image files
# Solution: Check image paths and file extensions
ls data/images/ | head -20

# Issue: Database connection error
# Solution: Verify PostgreSQL is running and DATABASE_URL is correct
sudo systemctl status postgresql
psql -U postgres -d eshopdb -c "SELECT version();"
```

---

### 2. validate_setup.py

**Purpose**: Validate environment, dependencies, database, and models before running experiments.

**Location**: `scripts/validate_setup.py`

**Parameters**: None (uses environment configuration)

**Usage**:

```bash
# Run full validation
python scripts/validate_setup.py

# Redirect output to file
python scripts/validate_setup.py > validation_report.txt 2>&1
```

**Checks Performed**:

1. **Environment Check**
   - Python version (≥3.9)
   - PyTorch installation
   - CUDA availability
   - Required packages

2. **Model Mapping Check**
   - Validates all 5 model mappings
   - Checks dimension consistency
   - Verifies prefix mappings

3. **Database Check**
   - PostgreSQL connection
   - pgvector extension
   - Table schema validation
   - Embedding column verification

4. **Dataset Check**
   - Metadata file existence
   - Image directory validation
   - Sample image verification

5. **Model Loading Check**
   - Loads all 5 champion models
   - Tests feature extraction
   - Validates output dimensions

**Output Example**:

```
======================================================================
 Fashion Image Search System - Setup Validation
 5 Champion Models: EfficientNet, ConvNeXt, CLIP, Fashion-CLIP, DINO
======================================================================
Project root: /home/user/fashion-image-search

======================================================================
 Environment Check
======================================================================
✓ Python version: 3.11.5
✓ PyTorch version: 2.1.0+cu118
✓ CUDA available: NVIDIA GeForce RTX 3080
✓ Transformers version: 4.35.2

======================================================================
 Model Mapping Check
======================================================================
✓ efficientnet_b0: prefix=efficientnet, col=embedding_efficientnet, dim=1280
✓ convnext_tiny: prefix=convnext, col=embedding_convnext, dim=768
✓ clip_vit_b16: prefix=clip, col=embedding_clip, dim=512
✓ fashion_clip: prefix=fclip, col=embedding_fclip, dim=512
✓ dinov2_vits14: prefix=dino, col=embedding_dino, dim=384

======================================================================
 Database Check
======================================================================
✓ PostgreSQL connected
  Version: PostgreSQL 14.9
✓ pgvector extension is active
✓ Database tables exist
✓ All 5 embedding columns present: ['embedding_efficientnet', 'embedding_convnext', 'embedding_clip', 'embedding_fclip', 'embedding_dino']

======================================================================
 Dataset Check
======================================================================
✓ Data directory exists: /home/user/fashion-image-search/data
✓ Metadata file found: data/styles.csv
  Sample IDs: [1163, 1164, 1165, 1166, 1525]
✓ Images directory exists: data/images
✓ Found 5/5 sample images

======================================================================
 Model Loading Check (5 Champion Models)
======================================================================
Champion models to test: ['efficientnet_b0', 'convnext_tiny', 'clip_vit_b16', 'fashion_clip', 'dinov2_vits14']
Device: CUDA

──────────────────────────────────────────────────────────────────────
Testing: efficientnet_b0
──────────────────────────────────────────────────────────────────────
📊 Loading efficientnet_b0...
✅ efficientnet_b0 loaded successfully
✓ SUCCESS: efficientnet_b0 loaded
  - Dimension: 1280
  - Device: cuda:0
  - Model name: efficientnet_b0
  - Feature extraction: ✓ (extracted 1280 dims)

[... similar output for other 4 models ...]

5/5 models loaded successfully

======================================================================
 Validation Summary
======================================================================
✓ Environment: PASSED
✓ Model Mapping: PASSED
✓ Database: PASSED
✓ Dataset: PASSED
✓ Models: PASSED

======================================================================
✓ All checks passed! System is ready.

Next steps:
  1. Load dataset:
     python -m app.dataset_loader --json data/styles.csv --images data/images --total 1000 --clear
  2. Start API:
     uvicorn app.main:app --reload --host 0.0.0.0 --port 8080
  3. Test API:
     python scripts/test_api.py
```

---

### 3. verify_models.py

**Purpose**: Test individual model loading and feature extraction.

**Location**: `scripts/verify_models.py`

**Parameters**: None (tests all models in config)

**Usage**:

```bash
# Test all models
python scripts/verify_models.py

# With verbose output
python scripts/verify_models.py 2>&1 | tee model_verification.log
```

**Output**:

```
======================================================================
 Model Loading Verification
 Testing 5 Champion Models
======================================================================

Models to test: ['efficientnet_b0', 'convnext_tiny', 'clip_vit_b16', 'fashion_clip', 'dinov2_vits14']
Device: CUDA

──────────────────────────────────────────────────────────────────────
Testing: efficientnet_b0
──────────────────────────────────────────────────────────────────────
✓ SUCCESS: efficientnet_b0 loaded
  - Dimension: 1280
  - Device: cuda:0
  - Model name: efficientnet_b0
  - Feature extraction: ✓ (extracted 1280 dims)

[... tests for other models ...]

======================================================================
 FINAL STATUS
======================================================================
efficientnet_b0     : ✓ OK
convnext_tiny       : ✓ OK
clip_vit_b16        : ✓ OK
fashion_clip        : ✓ OK
dinov2_vits14       : ✓ OK

Result: 5/5 models loaded successfully

✓ All models loaded successfully!

You can now proceed with dataset loading:
  python -m app.dataset_loader --json data/styles.csv --images data/images --total 1000
```

---

### 4. debug_clip_db.py

**Purpose**: Comprehensive debugging for database and CLIP models.

**Location**: `scripts/debug_clip_db.py`

**Parameters**: None

**Usage**:

```bash
python scripts/debug_clip_db.py
```

**What It Debugs**:

1. Database connection and schema
2. pgvector extension
3. Embedding columns in product_images table
4. OpenAI CLIP model loading
5. Fashion-CLIP model loading (Hugging Face)
6. All 5 champion models
7. Model mapping utility

**Output Sections**:

```
======================================================================
 FASHION IMAGE SEARCH - COMPREHENSIVE DEBUG
======================================================================

======================================================================
 DATABASE CONNECTION DEBUG
======================================================================
✓ PostgreSQL Version: PostgreSQL 14.9 (Ubuntu 14.9-0ubuntu0.22.04.1)
✓ pgvector extension is enabled
✓ Found 5 embedding columns:
  - embedding_efficientnet (USER-DEFINED)
  - embedding_convnext (USER-DEFINED)
  - embedding_clip (USER-DEFINED)
  - embedding_fclip (USER-DEFINED)
  - embedding_dino (USER-DEFINED)

✓ All 5 champion model columns present!

======================================================================
 MODEL MAPPING DEBUG
======================================================================

Model to Prefix Mapping:
  efficientnet_b0      → efficientnet
  convnext_tiny        → convnext
  clip_vit_b16         → clip
  fashion_clip         → fclip
  dinov2_vits14        → dino

Prefix to Model Mapping:
  efficientnet    → efficientnet_b0
  convnext        → convnext_tiny
  clip            → clip_vit_b16
  fclip           → fashion_clip
  dino            → dinov2_vits14

Model Dimensions:
  efficientnet_b0      → 1280 dims
  convnext_tiny        → 768 dims
  clip_vit_b16         → 512 dims
  fashion_clip         → 512 dims
  dinov2_vits14        → 384 dims

✓ Model mapping utility is working

======================================================================
 CLIP MODELS DEBUG
======================================================================

1. Testing OpenAI CLIP (via clip library)...
✓ clip library imported
  Device: cuda
✓ OpenAI CLIP ViT-B/16 loaded successfully
  Model on device: cuda:0

2. Testing Fashion-CLIP (via transformers)...
✓ transformers library imported
  Loading processor from patrickjohncyh/fashion-clip...
✓ Fashion-CLIP processor loaded
  Loading model from patrickjohncyh/fashion-clip...
✓ Fashion-CLIP model loaded
✓ Model moved to device: cuda

[... continues with all models ...]

======================================================================
 DEBUG COMPLETE
======================================================================

Next steps:
  1. If all checks passed, run: python scripts/validate_setup.py
  2. Load dataset: python -m app.dataset_loader --json data/styles.csv --images data/images
  3. Start API: uvicorn app.main:app --reload
```

---

### 5. thesis_validation_suite.py

**Purpose**: Comprehensive validation for thesis experiments with all metrics.

**Location**: `scripts/thesis_validation_suite.py`

**Parameters**:

```python
ThesisValidator(
    output_dir: str = "results/thesis_validation"  # Output directory
)

TEST_CONFIGS = {
    "sample_size": 100,              # Number of test queries
    "top_k_values": [5, 10, 20],    # K values for P@K, R@K metrics
    "confidence_level": 0.95,        # Statistical confidence
    "min_category_samples": 5        # Min samples per category
}
```

**Usage**:

```bash
# Run full validation suite
python scripts/thesis_validation_suite.py

# Customize output directory
python scripts/thesis_validation_suite.py --output results/experiment_1

# Customize sample size
python scripts/thesis_validation_suite.py --sample-size 200
```

**Validation Phases**:

**Phase 1: Search Feature Validation**
- Tests 3 search models: EfficientNet-B0, Fashion-CLIP, DINOv2
- Metrics: P@5, P@10, P@20, R@5, R@10, R@20, mAP@5/10/20
- Category-level breakdown

**Phase 2: Recommendation Feature Validation**
- Tests CLIP ViT-B/16
- Metrics: Same-category ratio, diversity score, average distance

**Phase 3: Statistical Comparison**
- Paired t-tests between models
- Cohen's d effect sizes
- Significance testing (p < 0.05)

**Phase 4: Inference Performance**
- Measures average, P95, throughput
- Compares search vs recommendation models

**Phase 5: Report Generation**
- Generates comprehensive markdown report
- Exports CSV files for analysis

**Output Structure**:

```
results/thesis_validation/
├── search_metrics.csv                    # Global metrics
├── search_category_breakdown.csv         # Per-category metrics
├── recommendation_metrics.csv            # Recommendation quality
├── statistical_comparison.csv            # Pairwise comparisons
├── performance_benchmarks.csv            # Inference speed
└── THESIS_REPORT.md                      # Comprehensive report
```

**Example Output**:

```
================================================================================
THESIS VALIDATION SUITE - FASHION IMAGE SEARCH
================================================================================

📊 PHASE 1: Search Feature Validation
Testing models: ['efficientnet_b0', 'fashion_clip', 'dinov2_vits14']
  🔍 Evaluating efficientnet_b0...
  🔍 Evaluating fashion_clip...
  🔍 Evaluating dinov2_vits14...

🎯 PHASE 2: Recommendation Feature Validation
Testing model: clip_vit_b16
  ✓ Relevance: 0.723
  ✓ Diversity: 0.456

⚖️ PHASE 3: Statistical Comparison
  ✓ 3 pairwise comparisons completed

⚡ PHASE 4: Inference Performance
  ⚡ Benchmarking efficientnet_b0...
  ⚡ Benchmarking fashion_clip...
  ⚡ Benchmarking dinov2_vits14...
  ⚡ Benchmarking clip_vit_b16...

📝 PHASE 5: Generating Reports
  ✓ Report saved: results/thesis_validation/THESIS_REPORT.md

✅ Validation complete! Results: results/thesis_validation
```

**Generated Report Sample**:

```markdown
# Fashion Image Search - Thesis Validation Report

Generated: 2025-12-23 15:30:45

## 1. Search Feature Performance

### Selected Models
- **efficientnet_b0**
- **fashion_clip**
- **dinov2_vits14**

### Global Metrics (mAP@10)

| Model | K | mP | mR | mAP |
|-------|---|----|----|-----|
| fashion_clip | 10 | 0.742 | 0.168 | 0.685 |
| efficientnet_b0 | 10 | 0.698 | 0.152 | 0.641 |
| dinov2_vits14 | 10 | 0.671 | 0.145 | 0.618 |

## 2. Recommendation Feature Performance

### Selected Model: clip_vit_b16

- **Relevance**: 0.723
- **Diversity**: 0.456
- **Avg Distance**: 0.234

## 3. Statistical Significance

| Model_A | Model_B | Difference | p_value | significant |
|---------|---------|------------|---------|-------------|
| fashion_clip | efficientnet_b0 | 0.044 | 0.023 | True |
| fashion_clip | dinov2_vits14 | 0.067 | 0.001 | True |
| efficientnet_b0 | dinov2_vits14 | 0.023 | 0.142 | False |

## 4. Inference Performance

| Model | Role | Avg_Inference_ms | Throughput_img_sec |
|-------|------|------------------|-------------------|
| efficientnet_b0 | Search | 45.3 | 22.1 |
| fashion_clip | Search | 89.2 | 11.2 |
| dinov2_vits14 | Search | 76.8 | 13.0 |
| clip_vit_b16 | Recommendation | 85.4 | 11.7 |

## 5. Recommendations

### For Production Deployment:
- **Search**: Use all 3 models in ensemble or select based on accuracy/speed trade-off
- **Recommendations**: Use clip_vit_b16 for balanced semantic understanding
```

---

### 6. test_thesis_api.py

**Purpose**: Comprehensive API testing for all thesis models and endpoints.

**Location**: `scripts/test_thesis_api.py`

**Parameters**:

```python
# Environment variables
API_BASE_URL = "http://127.0.0.1:8000"
API_KEY = "thesis-secure-api-key-2025"

# Test configuration
SEARCH_MODELS = ["efficientnet_b0", "fashion_clip", "dinov2_vits14"]
RECOMMENDATION_MODEL = "clip_vit_b16"

REQUIREMENTS = {
    'max_inference_time_ms': 500,
    'min_search_results': 5,
    'min_search_score': 0.5
}
```

**Usage**:

```bash
# Run all API tests
python scripts/test_thesis_api.py

# With custom API URL
API_BASE_URL=http://your-server:8000 python scripts/test_thesis_api.py

# Save results to file
python scripts/test_thesis_api.py > api_test_results.txt 2>&1
```

**Test Phases**:

```
Phase 1: Basic Connectivity
├── Test root endpoint
└── Test health check

Phase 2: Model Availability
└── Test /models endpoint for all 4 models

Phase 3: Sample Data Retrieval
└── Get sample product/image IDs from database

Phase 4: Search Feature Tests
├── Search by ID (efficientnet_b0)
├── Search by ID (fashion_clip)
├── Search by ID (dinov2_vits14)
├── Search by upload (efficientnet_b0)
├── Search by upload (fashion_clip)
└── Search by upload (dinov2_vits14)

Phase 5: Recommendation Tests
└── Recommendations by product ID (clip_vit_b16)

Phase 6: Cross-Model Comparison
└── Test /diagnostics/compare-models

Phase 7: Performance Validation
└── Validate inference times against requirements

Phase 8: Retrieval Metrics
└── Test /evaluation/metrics endpoint
```

**Output Example**:

```
======================================================================
 THESIS API INTEGRATION TESTS
 Fashion Image Search System
======================================================================

API URL: http://127.0.0.1:8000
Search Models: ['efficientnet_b0', 'fashion_clip', 'dinov2_vits14']
Recommendation Model: clip_vit_b16

📡 PHASE 1: Basic Connectivity
  → Testing root endpoint...
    ✓ Service: Fashion Image Search API
    ✓ Version: 3.5.0
  → Testing health check...
    ✓ Database: connected
    ✓ Indexed images: 4000
    ✓ All required models loaded

🤖 PHASE 2: Model Availability
  → Testing models endpoint...
    ✓ efficientnet_b0: CNN (1280D)
    ✓ fashion_clip: Transformer (512D)
    ✓ dinov2_vits14: Transformer (384D)
    ✓ clip_vit_b16: Transformer (512D)

📊 PHASE 3: Sample Data Retrieval
  → Retrieving sample data...
    ✓ Product ID: a1b2c3d4...
    ✓ Image ID: e5f6g7h8...

🔍 PHASE 4: Search Feature Tests (3 Models)
  → Testing search by ID (efficientnet_b0)...
    ✓ Results: 10
    ✓ Top score: 0.9876
    ✓ Time: 48.3ms
  → Testing search by upload (efficientnet_b0)...
    ✓ Results: 10, Time: 52.1ms
  → Testing search by ID (fashion_clip)...
    ✓ Results: 10
    ✓ Top score: 0.9654
    ✓ Time: 89.7ms
  → Testing search by upload (fashion_clip)...
    ✓ Results: 10, Time: 91.4ms
  → Testing search by ID (dinov2_vits14)...
    ✓ Results: 10
    ✓ Top score: 0.9432
    ✓ Time: 76.2ms
  → Testing search by upload (dinov2_vits14)...
    ✓ Results: 10, Time: 79.8ms

🎯 PHASE 5: Recommendation Feature Tests
  → Testing recommendations (clip_vit_b16)...
    ✓ Results: 10, Unique: 9

⚖️ PHASE 6: Cross-Model Comparison
  → Testing model comparison...
    ✓ All 3 models compared successfully
      - efficientnet_b0: 5 results
      - fashion_clip: 5 results
      - dinov2_vits14: 5 results

⚡ PHASE 7: Performance Validation
  → Validating performance requirements...
    ✓ efficientnet_b0: 48.3ms
    ✓ fashion_clip: 89.7ms
    ✓ dinov2_vits14: 76.2ms

📈 PHASE 8: Retrieval Metrics
  → Testing evaluation metrics endpoint...
    ✓ Metrics computed successfully
      - Global metrics: ['5', '10', '20']
      - Categories: 23

======================================================================
 TEST SUMMARY
======================================================================

Total Tests: 24
Passed: 24
Failed: 0
Success Rate: 100.0%

Basic Connectivity: 2/2
  ✓ root
  ✓ health
Model Availability: 1/1
  ✓ models_list
Search Feature: 6/6
  ✓ search_by_id_efficientnet_b0
  ✓ search_by_upload_efficientnet_b0
  ✓ search_by_id_fashion_clip
  ✓ search_by_upload_fashion_clip
  ✓ search_by_id_dinov2_vits14
  ✓ search_by_upload_dinov2_vits14
Recommendation Feature: 1/1
  ✓ recommendations_clip_vit_b16
Model Comparison: 1/1
  ✓ model_comparison
Performance: 1/1
  ✓ performance_requirements
Evaluation Metrics: 1/1
  ✓ evaluation_metrics

Detailed results saved to: results/api_tests/api_test_results.csv
```

---

### 7. run_full_experiment.py

**Purpose**: Automated thesis experiment runner with multiple dataset sizes.

**Location**: `evaluation/run_full_experiment.py`

**Parameters**:

```python
ThesisExperimentRunner(
    sizes: list,              # Dataset sizes to test [100, 500, 1000, 4000]
    models: list,             # Models to evaluate
    output_root: str          # Base output directory
)
```

**Usage**:

```bash
# Run experiments with default sizes
python evaluation/run_full_experiment.py --sizes 100 500 1000

# Full thesis experiment
python evaluation/run_full_experiment.py --sizes 100 500 1000 4000 --output docs/thesis/results

# Quick test run
python evaluation/run_full_experiment.py --sizes 100
```

**What It Does**:

For each dataset size:
1. Loads/clears database with specified size
2. Evaluates retrieval accuracy (P@K, R@K, mAP)
3. Benchmarks inference performance
4. Generates t-SNE visualizations
5. Creates comparison plots
6. Writes summary report

**Output Structure**:

```
docs/thesis/results/run_20251223_153045/
├── size_100/
│   ├── accuracy_metrics.csv
│   ├── performance_benchmarks.csv
│   ├── plots/
│   │   ├── accuracy_comparison.png
│   │   ├── efficiency_tradeoff.png
│   │   ├── tsne_efficientnet_b0.png
│   │   ├── tsne_fashion_clip.png
│   │   └── tsne_dinov2_vits14.png
│   └── SUMMARY.md
├── size_500/
│   └── [similar structure]
├── size_1000/
│   └── [similar structure]
└── experiment.log
```

**Example Output**:

```
================================================================================
FASHION IMAGE SEARCH - THESIS EXPERIMENT RUNNER
Models: ['efficientnet_b0', 'fashion_clip', 'dinov2_vits14', 'clip_vit_b16']
Dataset Sizes: [100, 500, 1000]
================================================================================
🔥 Pre-warming champion models...

>>> Starting Experiment: Size=100 <<<
1. Loading/Clearing database with 100 images...
   Selected 100 balanced items for import.
   Importing Products: 100%|████████| 100/100 [2:15<00:00,  0.74it/s]
   ✅ Import complete!

2. Evaluating retrieval accuracy (P@K, R@K, mAP)...
   🧠 Evaluating accuracy: efficientnet_b0
   🧠 Evaluating accuracy: fashion_clip
   🧠 Evaluating accuracy: dinov2_vits14

3. Benchmarking inference performance...
   ⚡ Testing efficientnet_b0...
   ⚡ Testing fashion_clip...
   ⚡ Testing dinov2_vits14...
   ⚡ Testing clip_vit_b16...

4. Generating thesis-ready visualizations...
   📊 Plotting accuracy comparison...
   📊 Plotting efficiency tradeoff...
   📊 Generating t-SNE for efficientnet_b0...
   📊 Generating t-SNE for fashion_clip...
   📊 Generating t-SNE for dinov2_vits14...

Summary written to: docs/thesis/results/run_20251223_153045/size_100/SUMMARY.md

>>> Starting Experiment: Size=500 <<<
[... similar output ...]

✅ All experiments completed. Results in: docs/thesis/results/run_20251223_153045
```

**Generated SUMMARY.md Example**:

```markdown
# 📊 Thesis Experiment: 1,000 Items

Generated on 2025-12-23 15:45:30

## 🎯 Retrieval Accuracy (@K=10)

| Model | K | mP | mR | mAP |
|-------|---|----|----|-----|
| fashion_clip | 10 | 0.756 | 0.172 | 0.698 |
| efficientnet_b0 | 10 | 0.712 | 0.158 | 0.654 |
| dinov2_vits14 | 10 | 0.689 | 0.149 | 0.631 |

## ⚡ Inference Performance

| Model | Type | Avg_Inference_ms | Throughput_img_sec |
|-------|------|------------------|-------------------|
| efficientnet_b0 | CNN | 46.2 | 21.6 |
| fashion_clip | Transformer | 88.7 | 11.3 |
| dinov2_vits14 | Transformer | 75.9 | 13.2 |
| clip_vit_b16 | Transformer | 84.3 | 11.9 |
```

---

### 8. metrics.py

**Purpose**: Calculate thesis evaluation metrics (P@K, R@K, mAP).

**Location**: `evaluation/metrics.py`

**Parameters**:

```python
SAMPLE_SIZE = 50              # Number of test queries
TOP_K_VALUES = [5, 10, 20]    # K values to evaluate
MODELS = settings.AVAILABLE_MODELS  # All 5 champion models
```

**Usage**:

```bash
# Run evaluation
python evaluation/metrics.py

# Redirect to file
python evaluation/metrics.py > evaluation_results.txt
```

**Metrics Calculated**:

- **Precision@K (P@K)**: Fraction of top-K results that are relevant
- **Recall@K (R@K)**: Fraction of all relevant items found in top-K
- **Average Precision (AP)**: Weighted precision across all relevant items
- **Mean Average Precision (mAP)**: Average AP across all queries

**Output**:

```
Evaluating efficientnet_b0...
Evaluating fashion_clip...
Evaluating dinov2_vits14...
Evaluating clip_vit_b16...
Evaluating convnext_tiny...

--- Category Breakdown for efficientnet_b0 @K=10 ---
           Name  mP@10  mR@10  mAP@10   N
    Tshirts    0.820  0.185   0.759  12
        Jeans    0.780  0.168   0.722  10
      Shirts    0.745  0.159   0.698   9
     Sandals    0.710  0.145   0.665   8
[... more categories ...]

================================================================================
FINAL THESIS COMPARATIVE ANALYSIS
================================================================================
           Model   K     mP     mR    mAP
  fashion_clip  10  0.756  0.172  0.698
efficientnet_b0  10  0.712  0.158  0.654
 dinov2_vits14  10  0.689  0.149  0.631
  clip_vit_b16  10  0.701  0.153  0.645
 convnext_tiny  10  0.678  0.142  0.619

✅ Results saved to results/thesis_evaluation.csv
```

---

### 9. visualize_metrics.py

**Purpose**: Generate thesis-ready visualizations (charts, t-SNE plots).

**Location**: `evaluation/visualize_metrics.py`

**Parameters**:

```python
ThesisVisualizer(
    output_dir: Path  # Directory for saving plots
)
```

**Usage**:

```bash
# Run as standalone to test
python evaluation/visualize_metrics.py

# Use in scripts
from evaluation.visualize_metrics import ThesisVisualizer, benchmark_models

viz = ThesisVisualizer(Path("results/visuals"))
viz.plot_accuracy_comparison(results_df)
viz.plot_inference_performance(bench_df)
viz.generate_tsne(db, "fashion_clip", n_samples=500)
```

**Generated Plots**:

1. **accuracy_comparison.png**: Bar chart of mAP@10 by model
2. **efficiency_tradeoff.png**: Scatter plot of inference time vs accuracy
3. **tsne_{model}.png**: 2D embedding visualization for each model

**Plot Examples**:

```
plots/
├── accuracy_comparison.png
│   └── Bar chart: mAP@10 for CNN vs Transformers
│       X-axis: Model names
│       Y-axis: mAP@10 score
│       Colors: Blue (CNN), Orange (Transformer)
│
├── efficiency_tradeoff.png
│   └── Scatter plot: Performance vs Accuracy
│       X-axis: Inference time (ms)
│       Y-axis: mAP@10
│       Points: Each model with labels
│
└── tsne_fashion_clip.png
    └── 2D scatter plot of embedding space
        Colors: Top 10 product categories
        Shows clustering of similar items
```

---

## API Documentation

### Base URL

```
http://localhost:8000
```

### Authentication

All protected endpoints require API key in header:

```bash
X-API-Key: thesis-secure-api-key-2025
```

---

### Endpoints

#### 1. GET /

**Description**: Root endpoint with API information.

**Authentication**: None required

**Response**:

```json
{
  "service": "Fashion Image Search API",
  "version": "3.5.0",
  "active_model": "efficientnet_b0",
  "available_models": [
    "efficientnet_b0",
    "convnext_tiny",
    "clip_vit_b16",
    "fashion_clip",
    "dinov2_vits14"
  ],
  "loaded_models": [
    "efficientnet_b0",
    "fashion_clip",
    "dinov2_vits14",
    "clip_vit_b16"
  ]
}
```

**cURL Example**:

```bash
curl http://localhost:8000/
```

---

#### 2. GET /health

**Description**: Health check with model status.

**Authentication**: None required

**Response**:

```json
{
  "status": "healthy",
  "database": "connected",
  "indexed_images": 4000,
  "models": [
    {
      "model_name": "efficientnet_b0",
      "architecture": "CNN",
      "loaded": true,
      "dimensions": 1280
    },
    {
      "model_name": "fashion_clip",
      "architecture": "Transformer",
      "loaded": true,
      "dimensions": 512
    }
  ]
}
```

**cURL Example**:

```bash
curl http://localhost:8000/health
```

---

#### 3. GET /models

**Description**: List all available models and their status.

**Authentication**: Required

**Response**:

```json
{
  "total_models": 5,
  "default_model": "efficientnet_b0",
  "models": {
    "efficientnet_b0": {
      "model_name": "efficientnet_b0",
      "architecture": "CNN",
      "loaded": true,
      "dimensions": 1280
    },
    "fashion_clip": {
      "model_name": "fashion_clip",
      "architecture": "Transformer",
      "loaded": true,
      "dimensions": 512
    }
  }
}
```

**cURL Example**:

```bash
curl -H "X-API-Key: thesis-secure-api-key-2025" \
  http://localhost:8000/models
```

---

#### 4. POST /search/by-upload

**Description**: Search for similar products by uploading an image.

**Authentication**: Required

**Parameters**:
- `file` (form-data): Image file (JPEG, PNG)
- `limit` (query, optional): Number of results (default: 10, max: 100)
- `model` (query, optional): Model to use (default: efficientnet_b0)

**Response**:

```json
{
  "model": "fashion_clip",
  "count": 10,
  "results": [
    {
      "product_id": "a1b2c3d4-e5f6-7890-gh12-ijklmn345678",
      "image_id": "p9q8r7s6-t5u4-3210-vw98-xyz765432109",
      "image_url": "data/images/12345.jpg",
      "score": 0.9876,
      "article_type": "Tshirts",
      "sub_category": "Topwear"
    }
  ]
}
```

**cURL Example**:

```bash
curl -X POST \
  -H "X-API-Key: thesis-secure-api-key-2025" \
  -F "file=@test_image.jpg" \
  "http://localhost:8000/search/by-upload?model=fashion_clip&limit=10"
```

**Python Example**:

```python
import requests

url = "http://localhost:8000/search/by-upload"
headers = {"X-API-Key": "thesis-secure-api-key-2025"}
files = {"file": open("test_image.jpg", "rb")}
params = {"model": "fashion_clip", "limit": 10}

response = requests.post(url, headers=headers, files=files, params=params)
results = response.json()
```

---

#### 5. GET /search/by-id/{image_id}

**Description**: Search for similar products using an existing image ID.

**Authentication**: Required

**Parameters**:
- `image_id` (path): UUID of source image
- `limit` (query, optional): Number of results (default: 10)
- `model` (query, optional): Model to use (default: efficientnet_b0)

**Response**: Same as /search/by-upload

**cURL Example**:

```bash
curl -H "X-API-Key: thesis-secure-api-key-2025" \
  "http://localhost:8000/search/by-id/e5f6g7h8-i9j0-1234-k567-lmn890123456?model=dinov2_vits14&limit=10"
```

**Python Example**:

```python
import requests

image_id = "e5f6g7h8-i9j0-1234-k567-lmn890123456"
url = f"http://localhost:8000/search/by-id/{image_id}"
headers = {"X-API-Key": "thesis-secure-api-key-2025"}
params = {"model": "dinov2_vits14", "limit": 10}

response = requests.get(url, headers=headers, params=params)
results = response.json()
```

---

#### 6. GET /recommendations/by-product-id/{product_id}

**Description**: Get product recommendations based on visual similarity.

**Authentication**: Required

**Parameters**:
- `product_id` (path): UUID of source product
- `limit` (query, optional): Number of recommendations (default: 10)
- `model` (query, optional): Model to use (default: clip_vit_b16)

**Response**:

```json
{
  "source_product_id": "a1b2c3d4-e5f6-7890-gh12-ijklmn345678",
  "model": "clip_vit_b16",
  "count": 10,
  "results": [
    {
      "product_id": "z9y8x7w6-v5u4-3210-t987-srqponm654321",
      "image_id": "o0n9m8l7-k6j5-4321-h098-gfedcba987654",
      "image_url": "data/images/67890.jpg",
      "score": 0.8765,
      "article_type": "Casual Shoes",
      "sub_category": "Shoes"
    }
  ]
}
```

**cURL Example**:

```bash
curl -H "X-API-Key: thesis-secure-api-key-2025" \
  "http://localhost:8000/recommendations/by-product-id/a1b2c3d4-e5f6-7890-gh12-ijklmn345678?limit=10"
```

---

#### 7. GET /diagnostics/compare-models/{image_id}

**Description**: Compare search results across multiple models for diagnostics.

**Authentication**: Required

**Parameters**:
- `image_id` (path): UUID of source image
- `models` (query, optional): List of models to compare
- `limit` (query, optional): Results per model (default: 5)

**Response**:

```json
{
  "source_image_id": "e5f6g7h8-i9j0-1234-k567-lmn890123456",
  "results_by_model": {
    "efficientnet_b0": {
      "model": "efficientnet_b0",
      "count": 5,
      "results": [...]
    },
    "fashion_clip": {
      "model": "fashion_clip",
      "count": 5,
      "results": [...]
    },
    "dinov2_vits14": {
      "model": "dinov2_vits14",
      "count": 5,
      "results": [...]
    }
  }
}
```

**cURL Example**:

```bash
curl -H "X-API-Key: thesis-secure-api-key-2025" \
  "http://localhost:8000/diagnostics/compare-models/e5f6g7h8-i9j0-1234-k567-lmn890123456?models=efficientnet_b0&models=fashion_clip&models=dinov2_vits14&limit=5"
```

---

#### 8. GET /evaluation/metrics

**Description**: Calculate thesis evaluation metrics (P@K, R@K, mAP) on test split.

**Authentication**: Required

**Parameters**:
- `model` (query, optional): Model to evaluate (default: efficientnet_b0)
- `sample_size` (query, optional): Number of test queries (default: 20, max: 100)

**Response**:

```json
{
  "model": "fashion_clip",
  "global_metrics": {
    "5": {
      "mP": 0.768,
      "mR": 0.142,
      "mAP": 0.712
    },
    "10": {
      "mP": 0.756,
      "mR": 0.172,
      "mAP": 0.698
    },
    "20": {
      "mP": 0.698,
      "mR": 0.231,
      "mAP": 0.654
    }
  },
  "category_breakdown": [
    {
      "category": "Tshirts",
      "mP_10": 0.820,
      "mR_10": 0.185,
      "mAP_10": 0.759,
      "sample_size": 12
    }
  ]
}
```

**cURL Example**:

```bash
curl -H "X-API-Key: thesis-secure-api-key-2025" \
  "http://localhost:8000/evaluation/metrics?model=fashion_clip&sample_size=50"
```

---

#### 9. POST /embeddings/generate

**Description**: Batch generate embeddings for specified images.

**Authentication**: Required

**Request Body**:

```json
{
  "image_ids": [
    "e5f6g7h8-i9j0-1234-k567-lmn890123456",
    "z9y8x7w6-v5u4-3210-t987-srqponm654321"
  ]
}
```

**Response**:

```json
{
  "successful": 2,
  "failed": 0,
  "details": [
    {
      "image_id": "e5f6g7h8-i9j0-1234-k567-lmn890123456",
      "status": "success",
      "updated_embeddings": [
        "fashion_clip",
        "dinov2_vits14"
      ]
    }
  ]
}
```

**cURL Example**:

```bash
curl -X POST \
  -H "X-API-Key: thesis-secure-api-key-2025" \
  -H "Content-Type: application/json" \
  -d '{"image_ids":["e5f6g7h8-i9j0-1234-k567-lmn890123456"]}' \
  http://localhost:8000/embeddings/generate
```

---

## Testing & Validation

### Test Workflow

```mermaid
graph TD
    A[Setup Environment] --> B[Validate Setup]
    B --> C[Load Dataset]
    C --> D[Start API Server]
    D --> E[Run API Tests]
    E --> F[Run Validation Suite]
    F --> G[Run Full Experiment]
    G --> H[Generate Thesis Report]
```

### Complete Test Sequence

```bash
# Step 1: Validate environment
python scripts/validate_setup.py

# Step 2: Load dataset (500 images for testing)
python -m app.dataset_loader \
  --json data/styles.csv \
  --images data/images \
  --total 500 \
  --clear

# Step 3: Start API server (in separate terminal)
uvicorn app.main:app --reload --host 0.0.0.0 --port 8000

# Step 4: Run API integration tests
python scripts/test_thesis_api.py

# Step 5: Run validation suite
python scripts/thesis_validation_suite.py

# Step 6: Run full experiment (optional)
python evaluation/run_full_experiment.py --sizes 500 1000

# Step 7: View results
cat results/thesis_validation/THESIS_REPORT.md
```

---

## Common Use Cases

### Use Case 1: Quick System Test

**Scenario**: Verify system is working after installation.

```bash
# 1. Validate setup
python scripts/validate_setup.py

# 2. Load small dataset
python -m app.dataset_loader --json data/styles.csv --images data/images --total 100 --clear

# 3. Start API
uvicorn app.main:app --reload &

# 4. Test API
python scripts/test_thesis_api.py

# Expected: All tests pass
```

---

### Use Case 2: Search Similar Products

**Scenario**: Find visually similar fashion items.

**Option A: Using API**

```python
import requests

# Upload image
url = "http://localhost:8000/search/by-upload"
headers = {"X-API-Key": "thesis-secure-api-key-2025"}
files = {"file": open("my_shirt.jpg", "rb")}
params = {"model": "fashion_clip", "limit": 10}

response = requests.post(url, headers=headers, files=files, params=params)
results = response.json()

for i, item in enumerate(results['results'], 1):
    print(f"{i}. {item['article_type']} - Score: {item['score']:.4f}")
```

**Option B: Using Database Directly**

```python
from app.database import SessionLocal, ProductImage
from app.model_factory import model_manager
from app.services.search_service import SearchService

db = SessionLocal()
embedder = model_manager.get_embedder("fashion_clip")

# Extract features from query image
vector = embedder.extract_features("my_shirt.jpg")

# Search
results = SearchService.search_by_vector(db, vector, "fashion_clip", limit=10)

for r in results:
    print(f"{r.article_type} - {r.score:.4f}")
```

---

### Use Case 3: Generate Recommendations

**Scenario**: Recommend products based on a viewed item.

```python
import requests

product_id = "a1b2c3d4-e5f6-7890-gh12-ijklmn345678"
url = f"http://localhost:8000/recommendations/by-product-id/{product_id}"
headers = {"X-API-Key": "thesis-secure-api-key-2025"}
params = {"model": "clip_vit_b16", "limit": 10}

response = requests.get(url, headers=headers, params=params)
recommendations = response.json()

print(f"Recommendations for product {product_id}:")
for i, rec in enumerate(recommendations['results'], 1):
    print(f"{i}. {rec['article_type']} - {rec['sub_category']} (Score: {rec['score']:.4f})")
```

---

### Use Case 4: Compare Model Performance

**Scenario**: Compare CNN vs Transformer for a specific query.

```python
import requests

image_id = "e5f6g7h8-i9j0-1234-k567-lmn890123456"
url = f"http://localhost:8000/diagnostics/compare-models/{image_id}"
headers = {"X-API-Key": "thesis-secure-api-key-2025"}
params = {
    "models": ["efficientnet_b0", "fashion_clip", "dinov2_vits14"],
    "limit": 5
}

response = requests.get(url, headers=headers, params=params)
comparison = response.json()

for model, data in comparison['results_by_model'].items():
    print(f"\n{model} ({data['count']} results):")
    for r in data['results']:
        print(f"  - {r['article_type']}: {r['score']:.4f}")
```

---

### Use Case 5: Evaluate Model Accuracy

**Scenario**: Calculate mAP@10 for thesis analysis.

```python
import requests

url = "http://localhost:8000/evaluation/metrics"
headers = {"X-API-Key": "thesis-secure-api-key-2025"}
params = {"model": "fashion_clip", "sample_size": 100}

response = requests.get(url, headers=headers, params=params)
metrics = response.json()

# Global metrics
print(f"Model: {metrics['model']}")
print(f"mAP@10: {metrics['global_metrics']['10']['mAP']:.4f}")
print(f"Precision@10: {metrics['global_metrics']['10']['mP']:.4f}")
print(f"Recall@10: {metrics['global_metrics']['10']['mR']:.4f}")

# Category breakdown
print("\nTop 5 Categories by mAP@10:")
top_cats = sorted(
    metrics['category_breakdown'], 
    key=lambda x: x['mAP_10'], 
    reverse=True
)[:5]
for cat in top_cats:
    print(f"  {cat['category']}: {cat['mAP_10']:.4f}")
```

---

### Use Case 6: Run
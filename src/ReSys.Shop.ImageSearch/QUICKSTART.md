# Quick Start Guide

Get the Fashion Image Search API running in 5 minutes!

## 🚀 Fast Track Setup

### Step 1: Prerequisites Check

```bash
# Check Python version (need 3.9+)
python --version

# Check PostgreSQL (need 14+)
psql --version

# Check Git
git --version
```

### Step 2: Clone and Setup

```bash
# Clone repository
git clone <repository-url>
cd fashion-image-search

# Create virtual environment
python -m venv venv
source venv/bin/activate  # Windows: venv\Scripts\activate

# Install dependencies
pip install -r requirements.txt
```

### Step 3: Database Setup

```bash
# Start PostgreSQL
sudo systemctl start postgresql  # Linux
# brew services start postgresql  # macOS

# Create database and user
sudo -u postgres psql << EOF
CREATE DATABASE eshopdb;
CREATE USER postgres WITH PASSWORD '12345678';
GRANT ALL PRIVILEGES ON DATABASE eshopdb TO postgres;
\q
EOF

# Enable pgvector
sudo -u postgres psql -d eshopdb -c "CREATE EXTENSION vector;"
```

### Step 4: Configure Environment

```bash
# Create .env file
cat > .env << EOF
DATABASE_URL=postgresql://postgres:12345678@localhost:5432/eshopdb
API_KEY=thesis-secure-api-key-2025
MODEL_TYPE=efficientnet_b0
UPLOAD_DIR=data/uploads
EOF
```

### Step 5: Download Dataset

```bash
# Option A: Kaggle (requires kaggle API)
pip install kaggle
# Setup: https://www.kaggle.com/docs/api
python scripts/download_dataset.py \
    --kaggle paramaggarwal/fashion-product-images-small \
    --dest data

# Option B: Manual
# 1. Download from: https://www.kaggle.com/paramaggarwal/fashion-product-images-small
# 2. Extract to data/ directory
```

### Step 6: Validate Setup

```bash
python scripts/validate_setup.py
```

Expected output:
```
✓ Environment: PASSED
✓ Database: PASSED
✓ Dataset: PASSED
✓ Models: PASSED
```

### Step 7: Load Data

```bash
# Load 4000 images (takes ~30-45 minutes)
python -m app.dataset_loader \
    --json data/styles.csv \
    --images data/images \
    --total 4000 \
    --clear
```

**Progress indicators:**
- Balancing dataset: 1-2 minutes
- Loading models: 2-3 minutes
- Importing products: 25-40 minutes
- Report metrics: 1 minute

### Step 8: Start API

```bash
# Development mode (with auto-reload)
uvicorn app.main:app --reload --host 0.0.0.0 --port 8000
```

API is now running at: http://localhost:8000

### Step 9: Test API

```bash
# In a new terminal
python scripts/test_api.py
```

### Step 10: Access Documentation

Open in browser:
- **Interactive API Docs**: http://localhost:8000/docs
- **Alternative Docs**: http://localhost:8000/redoc

## 🎯 Quick Test

### Using cURL

```bash
# Check health
curl http://localhost:8000/health

# List models
curl -H "X-API-Key: thesis-secure-api-key-2025" \
    http://localhost:8000/models

# Search (replace {IMAGE_ID} with actual ID from database)
curl -H "X-API-Key: thesis-secure-api-key-2025" \
    "http://localhost:8000/search/by-id/{IMAGE_ID}?model=clip&limit=5"
```

### Using Python

```python
import requests

API_URL = "http://localhost:8000"
HEADERS = {"X-API-Key": "thesis-secure-api-key-2025"}

# Health check
response = requests.get(f"{API_URL}/health")
print(response.json())

# List models
response = requests.get(f"{API_URL}/models", headers=HEADERS)
print(response.json())

# Upload search (with your own image)
with open("test_image.jpg", "rb") as f:
    files = {"file": f}
    response = requests.post(
        f"{API_URL}/search/by-upload",
        headers=HEADERS,
        params={"model": "efficientnet_b0", "limit": 10},
        files=files
    )
    print(response.json())
```

## 🔧 Troubleshooting

### Database Connection Failed

```bash
# Check if PostgreSQL is running
sudo systemctl status postgresql

# If not running, start it
sudo systemctl start postgresql

# Check connection
psql -U postgres -d eshopdb -c "SELECT 1;"
```

### CLIP Model Not Loading

This is OK! The system works with MobileNet and EfficientNet only:

```python
# Use alternative models in requests
params = {"model": "efficientnet_b0"}  # or "mobilenet_v3"
```

### Port 8000 Already in Use

```bash
# Use different port
uvicorn app.main:app --reload --port 8001

# Or kill existing process
lsof -ti:8000 | xargs kill -9
```

### Out of Memory

```bash
# Load smaller dataset
python -m app.dataset_loader \
    --json data/styles.csv \
    --images data/images \
    --total 1000 \
    --clear
```

## 📊 Running Evaluation

```bash
# Quick evaluation (20 queries)
python scripts/metrics.py --sample-size 20

# Full evaluation (100 queries)
python scripts/metrics.py --sample-size 100

# Results saved to: evaluation_results_TIMESTAMP.csv
```

## 🐳 Docker Alternative (Coming Soon)

```bash
# One command setup
docker-compose up -d

# That's it! API running at http://localhost:8000
```

## 📚 Next Steps

1. **Read the full README**: `README.md`
2. **Explore API docs**: http://localhost:8000/docs
3. **Run full evaluation**: `python scripts/metrics.py --sample-size 100`
4. **Customize models**: Edit `app/model_factory.py`

## 🆘 Getting Help

If you encounter issues:

1. Check `scripts/validate_setup.py` output
2. Review logs in console
3. Check API logs: `uvicorn` output
4. Database logs: `sudo tail -f /var/log/postgresql/postgresql-16-main.log`

## ⏱️ Expected Timings

| Task | Time |
|------|------|
| Setup environment | 5-10 min |
| Download dataset | 5-15 min |
| Load 4000 images | 30-45 min |
| Run evaluation (50 queries) | 2-5 min |
| **Total first-time setup** | **45-75 min** |

## 🎓 For Thesis Work

### Recommended Workflow

1. **Initial Setup** (do once)
   ```bash
   # Full 4000 image dataset
   python -m app.dataset_loader --json data/styles.csv --images data/images --total 4000 --clear
   ```

2. **Run Evaluations** (repeat for each experiment)
   ```bash
   # Evaluate all models
   python scripts/metrics.py --sample-size 100
   ```

3. **Compare Results**
   - Check generated CSV files
   - Use category breakdowns for analysis
   - Compare mAP@10 across models

4. **Document Findings**
   - Screenshot API docs
   - Save evaluation CSVs
   - Note inference times from logs

---

**Need help?** Check the main README.md for detailed documentation!
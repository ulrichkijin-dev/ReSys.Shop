# Use an official Python slim image for a smaller footprint
FROM python:3.11-slim as builder

# Set environment variables
ENV PYTHONDONTWRITEBYTECODE 1
ENV PYTHONUNBUFFERED 1
ENV PIP_NO_CACHE_DIR 1

WORKDIR /app

# Install system dependencies required for building some Python packages
RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential \
    libpq-dev \
    git \
    && rm -rf /var/lib/apt/lists/*

# Install dependencies
COPY requirements.txt .
RUN pip install --upgrade pip && \
    pip install --no-cache-dir -r requirements.txt

# --- Model Pre-download Stage ---
# We pre-download models to the container so it's ready to serve immediately
COPY app/ ./app/
COPY scripts/utils/verify_models.py ./scripts/utils/
RUN python -c "from app.model_factory import model_manager; \
    model_manager.warmup(['efficientnet_b0', 'clip_vit_b16', 'mobilenet_v3', 'dino_vit_s16'])"

# Final production image
FROM python:3.11-slim

WORKDIR /app

# Copy installed packages from builder
COPY --from=builder /usr/local/lib/python3.11/site-packages /usr/local/lib/python3.11/site-packages
COPY --from=builder /usr/local/bin /usr/local/bin
# Copy pre-downloaded models (cached in $HOME/.cache)
COPY --from=builder /root/.cache /root/.cache

# Copy application code
COPY . .

# Create upload directory
RUN mkdir -p data/uploads

# Expose FastAPI port
EXPOSE 8000

# Use uvicorn to run the app
CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000", "--workers", "1"]

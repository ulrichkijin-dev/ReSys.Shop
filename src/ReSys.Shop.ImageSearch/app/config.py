from pydantic_settings import BaseSettings, SettingsConfigDict
from typing import List, Optional
import os


class Settings(BaseSettings):
    # API Settings
    API_TITLE: str = "Fashion Image Search API"
    API_VERSION: str = "3.5.0"
    API_DESCRIPTION: str = "API for visual search, recommendation, and thesis evaluation metrics comparing CNN vs Transformer architectures."
    API_KEY: str = "thesis-secure-api-key-2025"
    API_KEY_NAME: str = "X-API-Key"

    # Model Settings (matching C# ProductImage entity)
    # These are the 5 champion models selected for the thesis
    DEFAULT_MODEL: str = "efficientnet_b0"
    AVAILABLE_MODELS: List[str] = [
        # Group A: Production Baseline CNN (1280-dim)
        "efficientnet_b0",  # EfficientNet-B0: Modern Production Default
        # Group B: Modern CNN Architecture (768-dim)
        "convnext_tiny",  # ConvNeXt-Tiny: Transformer-inspired CNN
        # Group C: General Semantic Transformer (512-dim)
        "clip_vit_b16",  # CLIP ViT-B/16: General Purpose Semantic Search
        # Group D: Domain-Specific Semantic Transformer (512-dim)
        "fashion_clip",  # Fashion-CLIP: Fine-tuned for Fashion Domain
        # Group E: Visual Structure Transformer (384-dim)
        "dinov2_vits14",  # DINOv2 ViT-S/14: Self-supervised Visual Features
    ]

    # Storage Settings
    UPLOAD_DIR: str = "data/uploads"

    # Database Settings
    DATABASE_URL: Optional[str] = None

    model_config = SettingsConfigDict(env_file=".env", extra="ignore")


settings = Settings()

# Ensure upload directory exists
os.makedirs(settings.UPLOAD_DIR, exist_ok=True)

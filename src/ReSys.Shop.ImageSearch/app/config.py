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
    
    # Model Settings
    DEFAULT_MODEL: str = "efficientnet_b0"
    AVAILABLE_MODELS: List[str] = [
        # Group A: Production Baselines (CNNs)
        "resnet50",           # The Industry Standard
        "mobilenet_v3",       # Mobile/Edge Optimized
        "efficientnet_b0",    # Modern Production Default (Champion)
        
        # Group B: Modern Architectures (CNNs)
        "convnext_tiny",      # Transformer-inspired CNN (Champion)
        
        # Group C: Semantic Transformers (Zero-Shot)
        "clip_vit_b16",       # Best General Search
        "fashion_clip",       # Best Fashion Domain Search (Champion)
        
        # Group D: Visual Transformers (Self-Supervised)
        "dino_vit_s16"        # Best Structure/Texture Matching (Champion)
    ]
    
    # Storage Settings
    UPLOAD_DIR: str = "data/uploads"
    
    # Database Settings
    DATABASE_URL: Optional[str] = None
    
    model_config = SettingsConfigDict(env_file=".env", extra="ignore")

settings = Settings()

# Ensure upload directory exists
os.makedirs(settings.UPLOAD_DIR, exist_ok=True)

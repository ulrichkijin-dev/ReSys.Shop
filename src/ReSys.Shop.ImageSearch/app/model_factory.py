"""
Model Factory for Fashion Image Search Thesis
==============================================

Implements embedding architectures for comparative analysis organized by groups:
- EfficientNet-B0: Production Baseline CNN
- ConvNeXt-Tiny: Modern CNN
- CLIP ViT-B/16: General Semantic Transformer
- Fashion-CLIP: Domain-Specific Transformer
- DINOv2 ViT-S/14: Visual Structure Transformer

Author: [Your Name]
Thesis: Building a Fashion E-commerce Application with Recommendation and Image-based Product Search
Date: December 2025
"""

import logging
from typing import Optional, Union, List, Dict, Type
from pathlib import Path
import io

import torch
import torch.nn as nn
from torchvision import models, transforms
from PIL import Image
import numpy as np
import clip

from app.config import settings

# Configure logging
logger = logging.getLogger(__name__)

# =============================================================================
# BASE EMBEDDER CLASS
# =============================================================================


class BaseEmbedder:
    """
    Abstract base class for all embedding models.
    """

    def __init__(self, name: str, dim: int):
        self.device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
        self.model = None
        self.preprocess = None
        self.name = name
        self.dim = dim

        device_name = (
            torch.cuda.get_device_name(0) if torch.cuda.is_available() else "CPU"
        )
        logger.info(f"[{self.name}] Computing device: {device_name}")

    def _load_image(
        self, image_input: Union[str, Path, Image.Image, bytes]
    ) -> Optional[Image.Image]:
        """Load image from various input types."""
        try:
            if isinstance(image_input, (str, Path)):
                return Image.open(image_input).convert("RGB")
            if isinstance(image_input, Image.Image):
                return image_input.convert("RGB")
            if isinstance(image_input, bytes):
                return Image.open(io.BytesIO(image_input)).convert("RGB")
        except Exception as e:
            logger.error(f"Failed to load image: {e}")
        return None

    def extract_features(
        self, image_input: Union[str, Path, Image.Image, bytes]
    ) -> Optional[List[float]]:
        """Extract normalized feature vector from an image."""
        image = self._load_image(image_input)
        if image is None:
            return None

        try:
            return self._forward(image)
        except Exception as e:
            logger.error(f"Feature extraction failed for {self.name}: {e}")
            return None

    def _normalize(self, features: torch.Tensor) -> List[float]:
        """Common normalization logic."""
        features = features.squeeze().cpu().numpy()
        norm = np.linalg.norm(features)
        normalized = features / (norm + 1e-9)
        return normalized.tolist()

    def _forward(self, image: Image.Image) -> List[float]:
        raise NotImplementedError("Subclasses must implement _forward()")


# =============================================================================
# CNN ARCHITECTURES
# =============================================================================


class EfficientNetEmbedder(BaseEmbedder):
    """EfficientNet-B0: Production Baseline CNN (1280-dim)"""

    def __init__(self):
        super().__init__("efficientnet_b0", 1280)
        logger.info(f"📊 Loading {self.name}...")

        try:
            weights = models.EfficientNet_B0_Weights.IMAGENET1K_V1
            full_model = models.efficientnet_b0(weights=weights)

            self.model = nn.Sequential(
                full_model.features, full_model.avgpool, nn.Flatten(1)
            ).to(self.device)

            self.model.eval()
            self.preprocess = weights.transforms()
            logger.info(f"✅ {self.name} loaded successfully")
        except Exception as e:
            logger.error(f"❌ Failed to load {self.name}: {e}")
            raise

    def _forward(self, image: Image.Image) -> List[float]:
        tensor = self.preprocess(image).unsqueeze(0).to(self.device)
        with torch.no_grad():
            features = self.model(tensor)
        return self._normalize(features)


class ConvNeXtTinyEmbedder(BaseEmbedder):
    """ConvNeXt-Tiny: Modern CNN with Transformer-like design (768-dim)"""

    def __init__(self):
        super().__init__("convnext_tiny", 768)
        logger.info(f"🧬 Loading {self.name}...")
        try:
            weights = models.ConvNeXt_Tiny_Weights.IMAGENET1K_V1
            full_model = models.convnext_tiny(weights=weights)
            self.model = nn.Sequential(
                full_model.features, full_model.avgpool, nn.Flatten(1)
            ).to(self.device)
            self.model.eval()
            self.preprocess = weights.transforms()
            logger.info(f"✅ {self.name} loaded successfully")
        except Exception as e:
            logger.error(f"❌ Failed to load {self.name}: {e}")
            raise

    def _forward(self, image: Image.Image) -> List[float]:
        tensor = self.preprocess(image).unsqueeze(0).to(self.device)
        with torch.no_grad():
            features = self.model(tensor)
        return self._normalize(features)


# =============================================================================
# TRANSFORMER ARCHITECTURES
# =============================================================================


class CLIPEmbedder(BaseEmbedder):
    """CLIP Vision Transformer: General Semantic Search (512-dim)"""

    def __init__(self, variant: str = "ViT-B/16"):
        name = "clip_vit_b16" if "16" in variant else "clip_vit_b32"
        super().__init__(name, 512)
        logger.info(f"🤖 Loading OpenAI CLIP {variant}...")

        try:
            self.model, self.preprocess = clip.load(variant, device=self.device)
            self.model.eval()
            logger.info(f"✅ {self.name} loaded successfully")
        except Exception as e:
            logger.error(f"❌ Failed to load CLIP: {e}")
            raise

    def _forward(self, image: Image.Image) -> List[float]:
        tensor = self.preprocess(image).unsqueeze(0).to(self.device)
        with torch.no_grad():
            features = self.model.encode_image(tensor)
        return self._normalize(features)


class FashionCLIPEmbedder(BaseEmbedder):
    """Fashion-CLIP: Domain-specific CLIP fine-tuned on fashion (512-dim)"""

    def __init__(self):
        super().__init__("fashion_clip", 512)
        logger.info(f"👗 Loading {self.name} (Hugging Face)...")
        try:
            from transformers import CLIPProcessor, CLIPModel

            model_id = "patrickjohncyh/fashion-clip"
            self.processor = CLIPProcessor.from_pretrained(model_id, use_fast=True)
            self.model = CLIPModel.from_pretrained(model_id).to(self.device)
            self.model.eval()
            logger.info(f"✅ {self.name} loaded successfully")
        except Exception as e:
            logger.error(f"❌ Failed to load Fashion-CLIP: {e}")
            raise

    def _forward(self, image: Image.Image) -> List[float]:
        inputs = self.processor(images=image, return_tensors="pt").to(self.device)
        with torch.no_grad():
            features = self.model.get_image_features(**inputs)
        return self._normalize(features)


class DINOEmbedder(BaseEmbedder):
    """DINOv2: Self-supervised Vision Transformer from Meta AI (384-dim)"""

    def __init__(self):
        super().__init__("dinov2_vits14", 384)
        logger.info(f"🦖 Loading {self.name} (Meta AI)...")

        try:
            # Loading DINOv2 Small (ViT-S/14)
            self.model = torch.hub.load(
                "facebookresearch/dinov2", "dinov2_vits14", pretrained=True
            ).to(self.device)

            self.model.eval()

            # DINOv2 standard preprocessing
            self.preprocess = transforms.Compose(
                [
                    transforms.Resize(
                        256, interpolation=transforms.InterpolationMode.BICUBIC
                    ),
                    transforms.CenterCrop(224),
                    transforms.ToTensor(),
                    transforms.Normalize(
                        mean=[0.485, 0.456, 0.406], std=[0.229, 0.224, 0.225]
                    ),
                ]
            )
            logger.info(f"✅ {self.name} loaded successfully")
        except Exception as e:
            logger.error(f"❌ Failed to load DINOv2: {e}")
            raise

    def _forward(self, image: Image.Image) -> List[float]:
        tensor = self.preprocess(image).unsqueeze(0).to(self.device)
        with torch.no_grad():
            features = self.model(tensor)
        return self._normalize(features)


# =============================================================================
# MODEL MANAGER (SINGLETON)
# =============================================================================


class ModelManager:
    """
    Manages loading and caching of embedding models.
    """

    _instance = None
    _embedders: Dict[str, BaseEmbedder] = {}

    _model_mapping: Dict[str, Type[BaseEmbedder]] = {
        # Canonical names (matching C# domain and config.py)
        "efficientnet_b0": EfficientNetEmbedder,
        "convnext_tiny": ConvNeXtTinyEmbedder,
        "clip_vit_b16": lambda: CLIPEmbedder("ViT-B/16"),
        "fashion_clip": FashionCLIPEmbedder,
        "dinov2_vits14": DINOEmbedder,
        # Aliases for backward compatibility
        "efficientnet": EfficientNetEmbedder,
        "convnext": ConvNeXtTinyEmbedder,
        "clip": lambda: CLIPEmbedder("ViT-B/16"),
        "fclip": FashionCLIPEmbedder,
        "dino": DINOEmbedder,
    }

    def __new__(cls):
        if cls._instance is None:
            cls._instance = super(ModelManager, cls).__new__(cls)
        return cls._instance

    def get_embedder(self, model_name: str) -> Optional[BaseEmbedder]:
        """Get or load an embedder by name."""
        if model_name in self._embedders:
            return self._embedders[model_name]

        if model_name not in self._model_mapping:
            logger.error(f"Unknown model: {model_name}")
            return None

        try:
            embedder = self._model_mapping[model_name]()
            self._embedders[model_name] = embedder
            return embedder
        except Exception as e:
            logger.error(f"Failed to initialize {model_name}: {e}")
            return None

    def get_loaded_models(self) -> Dict[str, BaseEmbedder]:
        """Return all currently loaded models."""
        return self._embedders

    def warmup(self, model_names: List[str] = None):
        """Pre-load specified models."""
        if model_names is None:
            model_names = [settings.DEFAULT_MODEL]

        for name in model_names:
            self.get_embedder(name)


# Global singleton instance
model_manager = ModelManager()


# Helper function for backward compatibility
def get_embedder(model_name: str) -> Optional[BaseEmbedder]:
    """Get an embedder instance by name."""
    return model_manager.get_embedder(model_name)


if __name__ == "__main__":
    logging.basicConfig(level=logging.INFO)
    manager = ModelManager()
    manager.warmup(["efficientnet_b0", "clip_vit_b16", "fashion_clip"])
    print(f"Loaded models: {list(manager.get_loaded_models().keys())}")

import os
import torch
import torch.nn as nn
from torchvision import models, transforms
from PIL import Image
import numpy as np

# Thesis Architecture Comparison Factory
# ---------------------------------------------------------
# 1. MobileNetV3: Representative of Efficient CNNs
# 2. EfficientNet: Representative of Scaled CNNs
# 3. ViT (Vision Transformer): Representative of Transformer Architectures
#    Note: Replaced CLIP with torchvision ViT-B/32 due to local environment restrictions.

class BaseEmbedder:
    def __init__(self):
        self.device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')
        self.model = None
        self.preprocess = None
        self.name = "base"
        self.dim = 0

    def extract_features(self, image_input) -> list:
        # Load image
        if isinstance(image_input, (str, bytes)):
            try:
                image = Image.open(image_input).convert('RGB')
            except Exception as e:
                print(f"Error opening image {image_input}: {e}")
                return None
        else:
            image = image_input.convert('RGB')
            
        return self._forward(image)

    def _forward(self, image):
        raise NotImplementedError

class MobileNetV3Embedder(BaseEmbedder):
    def __init__(self):
        super().__init__()
        self.name = "mobilenet_v3"
        self.dim = 576
        print(f"Loading {self.name} (Efficient CNN)...")
        weights = models.MobileNet_V3_Small_Weights.IMAGENET1K_V1
        full_model = models.mobilenet_v3_small(weights=weights)
        self.model = nn.Sequential(full_model.features, full_model.avgpool, nn.Flatten(1)).to(self.device)
        self.model.eval()
        self.preprocess = transforms.Compose([
            transforms.Resize(256),
            transforms.CenterCrop(224),
            transforms.ToTensor(),
            transforms.Normalize(mean=[0.485, 0.456, 0.406], std=[0.229, 0.224, 0.225])
        ])

    def _forward(self, image):
        tensor = self.preprocess(image).unsqueeze(0).to(self.device)
        with torch.no_grad():
            output = self.model(tensor)
        features = output.squeeze().cpu().numpy()
        return (features / (np.linalg.norm(features) + 1e-9)).tolist()

class EfficientNetEmbedder(BaseEmbedder):
    def __init__(self):
        super().__init__()
        self.name = "efficientnet_b0"
        self.dim = 1280
        print(f"Loading {self.name} (Scaled CNN)...")
        weights = models.EfficientNet_B0_Weights.IMAGENET1K_V1
        full_model = models.efficientnet_b0(weights=weights)
        self.model = nn.Sequential(full_model.features, full_model.avgpool, nn.Flatten(1)).to(self.device)
        self.model.eval()
        self.preprocess = transforms.Compose([
            transforms.Resize(256),
            transforms.CenterCrop(224),
            transforms.ToTensor(),
            transforms.Normalize(mean=[0.485, 0.456, 0.406], std=[0.229, 0.224, 0.225])
        ])

    def _forward(self, image):
        tensor = self.preprocess(image).unsqueeze(0).to(self.device)
        with torch.no_grad():
            output = self.model(tensor)
        features = output.squeeze().cpu().numpy()
        return (features / (np.linalg.norm(features) + 1e-9)).tolist()

class ViTEmbedder(BaseEmbedder):
    """
    Vision Transformer (ViT-B/32) implementation.
    Standard Transformer architecture for thesis comparison.
    """
    def __init__(self):
        super().__init__()
        self.name = "clip" # Keep name 'clip' for database/API compatibility
        self.dim = 768 # ViT-B/32 standard dim is 768
        print(f"Loading {self.name} (Vision Transformer ViT-B/32 from torchvision)...")
        
        try:
            weights = models.ViT_B_32_Weights.IMAGENET1K_V1
            full_model = models.vit_b_32(weights=weights)
            
            # The representation is the [CLS] token (first element of heads)
            # For feature extraction, we take the model up to the representation head
            self.model = full_model
            # Replace the final head with Identity to get the 768-dim features
            self.model.heads = nn.Identity()
            self.model.to(self.device)
            self.model.eval()
            
            self.preprocess = weights.transforms()
            print("Successfully loaded ViT Vision Model.")
        except Exception as e:
            print(f"Error initializing ViT: {e}")
            raise e

    def _forward(self, image):
        tensor = self.preprocess(image).unsqueeze(0).to(self.device)
        with torch.no_grad():
            output = self.model(tensor)
        
        # ViT output is (Batch, 768)
        features = output.squeeze().cpu().numpy()
        return (features / (np.linalg.norm(features) + 1e-9)).tolist()

_embedders = {}

def get_embedder(model_name: str):
    global _embedders
    if model_name not in _embedders:
        try:
            if model_name == "mobilenet_v3":
                _embedders[model_name] = MobileNetV3Embedder()
            elif model_name == "efficientnet_b0":
                _embedders[model_name] = EfficientNetEmbedder()
            elif model_name == "clip":
                _embedders[model_name] = ViTEmbedder()
            else:
                raise ValueError(f"Unknown model: {model_name}")
        except Exception as e:
            print(f"Error initializing {model_name}: {e}")
            return None
    return _embedders[model_name]
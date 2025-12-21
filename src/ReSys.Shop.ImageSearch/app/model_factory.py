import torch
import torch.nn as nn
from torchvision import models, transforms
from PIL import Image
import numpy as np
from transformers import CLIPProcessor, CLIPModel

# Thesis Architecture Comparison Factory
# ---------------------------------------------------------
# 1. MobileNetV3: Representative of Efficient CNNs (Depthwise Separable Convs)
# 2. EfficientNet: Representative of Scaled CNNs (Compound Scaling)
# 3. CLIP: Representative of Transformers (Attention + Language Alignment)

class BaseEmbedder:
    def __init__(self):
        self.device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')
        self.model = None
        self.preprocess = None
        self.name = "base"
        self.dim = 0

    def extract_features(self, image_input) -> list:
        # Load image
        if isinstance(image_input, str):
            image = Image.open(image_input).convert('RGB')
        else:
            image = image_input.convert('RGB')
            
        return self._forward(image)

    def _forward(self, image):
        raise NotImplementedError

class MobileNetV3Embedder(BaseEmbedder):
    """
    Architecture: CNN (Efficient)
    Key Feature: Depthwise Separable Convolutions + Squeeze-and-Excite
    Goal: Low latency, low parameter count.
    Output Dim: 576
    """
    def __init__(self):
        super().__init__()
        self.name = "mobilenet_v3"
        self.dim = 576
        print(f"Loading {self.name} (Efficient CNN)...")
        
        # Use Small variant for maximum efficiency contrast
        weights = models.MobileNet_V3_Small_Weights.IMAGENET1K_V1
        full_model = models.mobilenet_v3_small(weights=weights)
        
        # Strip classifier, keep features + pooling
        self.model = nn.Sequential(
            full_model.features,
            full_model.avgpool,
            nn.Flatten(1)
        ).to(self.device)
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
        
        # L2 Normalize
        features = output.squeeze().cpu().numpy()
        return (features / (np.linalg.norm(features) + 1e-9)).tolist()

class EfficientNetEmbedder(BaseEmbedder):
    """
    Architecture: CNN (Scaled)
    Key Feature: Compound Scaling (Depth, Width, Resolution)
    Goal: High accuracy per parameter.
    Output Dim: 1280 (B0)
    """
    def __init__(self):
        super().__init__()
        self.name = "efficientnet_b0"
        self.dim = 1280
        print(f"Loading {self.name} (Scaled CNN)...")
        
        # Use B0 as the baseline for compound scaling
        weights = models.EfficientNet_B0_Weights.IMAGENET1K_V1
        full_model = models.efficientnet_b0(weights=weights)
        
        self.model = nn.Sequential(
            full_model.features,
            full_model.avgpool,
            nn.Flatten(1)
        ).to(self.device)
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

class CLIPEmbedder(BaseEmbedder):
    """
    Architecture: Transformer (ViT)
    Key Feature: Multi-head Attention + Language Alignment
    Goal: Semantic understanding, Zero-shot capability.
    Output Dim: 512
    """
    def __init__(self):
        super().__init__()
        self.name = "clip"
        self.dim = 512
        print(f"Loading {self.name} (Transformer)...")
        
        # Use ViT-B/32 standard model
        self.model_id = "openai/clip-vit-base-patch32"
        self.processor = CLIPProcessor.from_pretrained(self.model_id)
        self.model = CLIPModel.from_pretrained(self.model_id).to(self.device)
        self.model.eval()

    def _forward(self, image):
        inputs = self.processor(images=image, return_tensors="pt", padding=True).to(self.device)
        with torch.no_grad():
            output = self.model.get_image_features(**inputs)
            
        features = output.squeeze().cpu().numpy()
        return (features / (np.linalg.norm(features) + 1e-9)).tolist()

# Factory Cache
_embedders = {}

def get_embedder(model_name: str):
    global _embedders
    if model_name not in _embedders:
        if model_name == "mobilenet_v3":
            _embedders[model_name] = MobileNetV3Embedder()
        elif model_name == "efficientnet_b0":
            _embedders[model_name] = EfficientNetEmbedder()
        elif model_name == "clip":
            _embedders[model_name] = CLIPEmbedder()
        else:
            raise ValueError(f"Unknown model: {model_name}")
    return _embedders[model_name]

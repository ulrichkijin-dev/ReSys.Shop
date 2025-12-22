import os
import sys
from pathlib import Path
import torch

# Add project root to path to import app
sys.path.append(str(Path(__file__).resolve().parent.parent.parent))

from app.model_factory import get_embedder

def test_models():
    models = ["mobilenet_v3", "efficientnet_b0", "clip_vit_b16", "dino_vit_s16"]
    results = {}
    
    print("=== Model Loading Verification ===")
    for m_name in models:
        print(f"\nAttempting to load: {m_name}")
        try:
            embedder = get_embedder(m_name)
            if embedder:
                print(f"SUCCESS: {m_name} loaded. Dimension: {embedder.dim}")
                results[m_name] = True
            else:
                print(f"FAILED: {m_name} returned None")
                results[m_name] = False
        except Exception as e:
            print(f"ERROR loading {m_name}: {e}")
            results[m_name] = False
            
    print("\n=== Final Status ===")
    all_ok = all(results.values())
    for m, status in results.items():
        print(f"{m:20s}: {'OK' if status else 'FAIL'}")
        
    if not all_ok:
        print("\nSome models failed to load. Please check your internet connection or Hugging Face token.")
        sys.exit(1)
    else:
        print("\nAll models loaded successfully!")

if __name__ == "__main__":
    test_models()
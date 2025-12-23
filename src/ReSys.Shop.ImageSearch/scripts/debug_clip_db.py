import os
import sys
from pathlib import Path
import torch
from sqlalchemy import text
from dotenv import load_dotenv

# Add project root to path to import app
sys.path.append(str(Path(__file__).resolve().parent.parent))

load_dotenv()


def debug_database():
    """Debug database connection and schema."""
    print("=" * 70)
    print(" DATABASE CONNECTION DEBUG")
    print("=" * 70)

    try:
        from app.database import engine, USE_SQLITE_DEV

        with engine.connect() as conn:
            if not USE_SQLITE_DEV:
                result = conn.execute(text("SELECT version();"))
                print(f"✓ PostgreSQL Version: {result.fetchone()[0]}")

                # Check for pgvector extension
                try:
                    conn.execute(text("CREATE EXTENSION IF NOT EXISTS vector;"))
                    conn.commit()
                    print("✓ pgvector extension is enabled")
                except Exception as ve:
                    print(f"✗ Warning: Could not ensure pgvector extension: {ve}")

                # Check if product_images table has all 5 embedding columns
                try:
                    result = conn.execute(
                        text("""
                        SELECT column_name, data_type 
                        FROM information_schema.columns 
                        WHERE table_schema = 'eshopdb'
                        AND table_name = 'product_images' 
                        AND column_name LIKE 'embedding_%'
                        AND data_type = 'USER-DEFINED';
                    """)
                    )
                    cols = result.fetchall()

                    if cols:
                        print(f"✓ Found {len(cols)} embedding columns:")
                        for col_name, data_type in cols:
                            print(f"  - {col_name} ({data_type})")

                        # Check for all 5 expected columns
                        expected = [
                            "embedding_efficientnet",
                            "embedding_convnext",
                            "embedding_clip",
                            "embedding_fclip",
                            "embedding_dino",
                        ]
                        found = [c[0] for c in cols]
                        missing = set(expected) - set(found)

                        if missing:
                            print(f"\n✗ Missing embedding columns: {missing}")
                        else:
                            print(f"\n✓ All 5 champion model columns present!")
                    else:
                        print(
                            "✗ Error: No embedding columns found in product_images table"
                        )

                except Exception as e:
                    print(f"✗ Error checking table structure: {e}")
            else:
                print("✓ Using SQLite (development mode)")

    except Exception as e:
        print(f"✗ CRITICAL: Database connection failed: {e}")
        import traceback

        traceback.print_exc()


def debug_clip_models():
    """Debug CLIP and Fashion-CLIP model loading."""
    print("\n" + "=" * 70)
    print(" CLIP MODELS DEBUG")
    print("=" * 70)

    # Test standard CLIP
    print("\n1. Testing OpenAI CLIP (via clip library)...")
    try:
        import clip

        print("✓ clip library imported")

        device = "cuda" if torch.cuda.is_available() else "cpu"
        print(f"  Device: {device}")

        model, preprocess = clip.load("ViT-B/16", device=device)
        print("✓ OpenAI CLIP ViT-B/16 loaded successfully")
        print(f"  Model on device: {next(model.parameters()).device}")

    except Exception as e:
        print(f"✗ Failed to load OpenAI CLIP: {e}")
        import traceback

        traceback.print_exc()

    # Test Fashion-CLIP
    print("\n2. Testing Fashion-CLIP (via transformers)...")
    model_id = "patrickjohncyh/fashion-clip"

    try:
        from transformers import CLIPProcessor, CLIPModel

        print("✓ transformers library imported")

        print(f"  Loading processor from {model_id}...")
        processor = CLIPProcessor.from_pretrained(model_id, use_fast=True)
        print("✓ Fashion-CLIP processor loaded")

        print(f"  Loading model from {model_id}...")
        model = CLIPModel.from_pretrained(model_id)
        print("✓ Fashion-CLIP model loaded")

        device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
        model = model.to(device)
        print(f"✓ Model moved to device: {device}")

    except Exception as e:
        print(f"✗ Failed to load Fashion-CLIP: {e}")
        print("\nTroubleshooting:")
        print("  1. Check internet connection")
        print("  2. Update transformers: pip install --upgrade transformers")
        print("  3. Clear cache: rm -rf ~/.cache/huggingface/")
        import traceback

        traceback.print_exc()


def debug_all_models():
    """Debug all 5 champion models."""
    print("\n" + "=" * 70)
    print(" ALL CHAMPION MODELS DEBUG")
    print("=" * 70)

    try:
        from app.model_factory import model_manager
        from app.config import settings

        print(f"\nTesting {len(settings.AVAILABLE_MODELS)} models:")
        print(f"Models: {settings.AVAILABLE_MODELS}\n")

        for model_name in settings.AVAILABLE_MODELS:
            print(f"{'─' * 70}")
            print(f"Testing: {model_name}")
            print(f"{'─' * 70}")

            try:
                embedder = model_manager.get_embedder(model_name)

                if embedder:
                    print(f"✓ {model_name} loaded successfully")
                    print(f"  - Dimension: {embedder.dim}")
                    print(f"  - Device: {embedder.device}")

                    # Try a dummy extraction
                    from PIL import Image
                    import numpy as np

                    test_img = Image.fromarray(
                        np.random.randint(0, 255, (224, 224, 3), dtype=np.uint8)
                    )
                    features = embedder.extract_features(test_img)

                    if features and len(features) == embedder.dim:
                        print(f"  - Feature extraction: ✓")
                    else:
                        print(f"  - Feature extraction: ✗ (dimension mismatch)")
                else:
                    print(f"✗ {model_name} failed (returned None)")

            except Exception as e:
                print(f"✗ {model_name} failed: {e}")
                import traceback

                traceback.print_exc()

            print()

    except Exception as e:
        print(f"✗ Critical error: {e}")
        import traceback

        traceback.print_exc()


def debug_model_mapping():
    """Debug model mapping utility."""
    print("\n" + "=" * 70)
    print(" MODEL MAPPING DEBUG")
    print("=" * 70)

    try:
        from app.model_mapping import (
            get_embedding_prefix,
            get_embedding_column,
            get_model_dimension,
            MODEL_TO_PREFIX,
            PREFIX_TO_MODEL,
            MODEL_DIMENSIONS,
        )

        print("\nModel to Prefix Mapping:")
        for model, prefix in MODEL_TO_PREFIX.items():
            print(f"  {model:20s} → {prefix}")

        print("\nPrefix to Model Mapping:")
        for prefix, model in PREFIX_TO_MODEL.items():
            print(f"  {prefix:15s} → {model}")

        print("\nModel Dimensions:")
        for model, dim in MODEL_DIMENSIONS.items():
            print(f"  {model:20s} → {dim} dims")

        print("\n✓ Model mapping utility is working")

    except Exception as e:
        print(f"✗ Model mapping error: {e}")
        import traceback

        traceback.print_exc()


if __name__ == "__main__":
    print("\n" + "=" * 70)
    print(" FASHION IMAGE SEARCH - COMPREHENSIVE DEBUG")
    print("=" * 70)

    debug_database()
    debug_model_mapping()
    debug_clip_models()
    debug_all_models()

    print("\n" + "=" * 70)
    print(" DEBUG COMPLETE")
    print("=" * 70)
    print("\nNext steps:")
    print("  1. If all checks passed, run: python scripts/validate_setup.py")
    print(
        "  2. Load dataset: python -m app.dataset_loader --json data/styles.csv --images data/images"
    )
    print("  3. Start API: uvicorn app.main:app --reload")

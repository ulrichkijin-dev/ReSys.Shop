import sys
from pathlib import Path
import torch

# Add project root to path to import app
sys.path.append(str(Path(__file__).resolve().parent.parent))

from app.model_factory import model_manager
from app.config import settings


def test_models():
    """Test loading all 5 champion models."""
    print("=" * 70)
    print(" Model Loading Verification")
    print(" Testing 5 Champion Models")
    print("=" * 70)

    models = settings.AVAILABLE_MODELS
    print(f"\nModels to test: {models}")
    print(f"Device: {'CUDA' if torch.cuda.is_available() else 'CPU'}")

    results = {}

    for m_name in models:
        print(f"\n{'─' * 70}")
        print(f"Testing: {m_name}")
        print(f"{'─' * 70}")

        try:
            embedder = model_manager.get_embedder(m_name)

            if embedder:
                print(f"✓ SUCCESS: {m_name} loaded")
                print(f"  - Dimension: {embedder.dim}")
                print(f"  - Device: {embedder.device}")
                print(f"  - Model name: {embedder.name}")

                # Test with a dummy image
                try:
                    from PIL import Image
                    import numpy as np

                    # Create a small test image
                    test_img = Image.fromarray(
                        np.random.randint(0, 255, (224, 224, 3), dtype=np.uint8)
                    )

                    features = embedder.extract_features(test_img)
                    if features and len(features) == embedder.dim:
                        print(
                            f"  - Feature extraction: ✓ (extracted {len(features)} dims)"
                        )
                        results[m_name] = True
                    else:
                        print("  - Feature extraction: ✗ (wrong dimension)")
                        results[m_name] = False

                except Exception as e:
                    print(f"  - Feature extraction: ✗ ({e})")
                    results[m_name] = False
            else:
                print(f"✗ FAILED: {m_name} returned None")
                results[m_name] = False

        except Exception as e:
            print(f"✗ ERROR loading {m_name}: {e}")
            import traceback

            traceback.print_exc()
            results[m_name] = False

    # Final Summary
    print(f"\n{'=' * 70}")
    print(" FINAL STATUS")
    print(f"{'=' * 70}")

    success_count = sum(results.values())
    total_count = len(results)

    for m, status in results.items():
        status_str = "✓ OK" if status else "✗ FAIL"
        print(f"{m:20s}: {status_str}")

    print(f"\nResult: {success_count}/{total_count} models loaded successfully")

    if success_count < total_count:
        print("\n⚠ Some models failed to load.")
        print("\nPossible solutions:")
        print("  1. Check internet connection (models download on first use)")
        print("  2. For Fashion-CLIP: Ensure transformers library is up to date")
        print(
            "  3. For DINO: torch.hub may need internet access to facebookresearch/dinov2"
        )
        print("  4. Check GPU memory if using CUDA")
        print(
            "  5. Try running with --no-cache-dir: pip install --no-cache-dir transformers"
        )
        sys.exit(1)
    else:
        print("\n✓ All models loaded successfully!")
        print("\nYou can now proceed with dataset loading:")
        print(
            "  python -m app.dataset_loader --json data/styles.csv --images data/images --total 1000"
        )
        sys.exit(0)


def test_model_mapping():
    """Test the centralized model mapping utility."""
    print("\n" + "=" * 70)
    print(" Model Mapping Utility Test")
    print("=" * 70)

    try:
        from app.model_mapping import (
            get_embedding_prefix,
            get_embedding_column,
            get_model_dimension,
            get_canonical_model_name,
        )

        test_cases = [
            ("efficientnet_b0", "efficientnet", 1280),
            ("convnext_tiny", "convnext", 768),
            ("clip_vit_b16", "clip", 512),
            ("fashion_clip", "fclip", 512),
            ("dinov2_vits14", "dino", 384),
        ]

        all_passed = True
        for model, expected_prefix, expected_dim in test_cases:
            prefix = get_embedding_prefix(model)
            col = get_embedding_column(model)
            dim = get_model_dimension(model)
            canonical = get_canonical_model_name(model)

            if prefix == expected_prefix and dim == expected_dim:
                print(f"✓ {model}: prefix={prefix}, col={col}, dim={dim}")
            else:
                print(f"✗ {model} FAILED")
                all_passed = False

        if all_passed:
            print("\n✓ Model mapping utility working correctly")
        else:
            print("\n✗ Model mapping has issues")
            sys.exit(1)

    except Exception as e:
        print(f"✗ Error testing model mapping: {e}")
        sys.exit(1)


if __name__ == "__main__":
    test_model_mapping()
    test_models()

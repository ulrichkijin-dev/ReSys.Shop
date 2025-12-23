"""
Environment Validation Script

Checks that all dependencies and configurations are properly set up
before running the image search system.
"""

import os
import sys
from pathlib import Path
import torch
import pandas as pd
from PIL import Image
from sqlalchemy import text

# Add project root to path
sys.path.append(str(Path(__file__).resolve().parent.parent))


def print_header(text):
    """Print a formatted section header."""
    print("\n" + "=" * 70)
    print(f" {text}")
    print("=" * 70)


def check_environment():
    """Check Python environment and dependencies."""
    print_header("Environment Check")
    print(f"✓ Python version: {sys.version.split()[0]}")
    print(f"✓ PyTorch version: {torch.__version__}")

    if torch.cuda.is_available():
        print(f"✓ CUDA available: {torch.cuda.get_device_name(0)}")
    else:
        print("⚠ CUDA not available - will use CPU (slower)")

    # Check transformers version
    try:
        import transformers

        print(f"✓ Transformers version: {transformers.__version__}")
    except ImportError:
        print("✗ Transformers not installed")
        return False

    return True


def check_database():
    """Check database connectivity and pgvector."""
    print_header("Database Check")

    try:
        from app.database import engine, SessionLocal

        with engine.connect() as conn:
            # Test connection
            result = conn.execute(text("SELECT version()"))
            version = result.fetchone()[0]
            print(f"✓ PostgreSQL connected")
            print(f"  Version: {version.split(',')[0]}")

            # Check pgvector
            try:
                conn.execute(text("CREATE EXTENSION IF NOT EXISTS vector"))
                conn.commit()
                conn.execute(text("SELECT '[1,2,3]'::vector"))
                print("✓ pgvector extension is active")
            except Exception as e:
                print(f"✗ pgvector check failed: {e}")
                print("  Install: sudo apt install postgresql-16-pgvector")
                return False

            # Check if tables exist
            result = conn.execute(
                text("""
                SELECT COUNT(*) 
                FROM information_schema.tables 
                WHERE table_schema = 'eshopdb' 
                AND table_name = 'products'
            """)
            )
            if result.scalar() > 0:
                print("✓ Database tables exist")

                # Check for all 5 embedding columns
                result = conn.execute(
                    text("""
                    SELECT column_name 
                    FROM information_schema.columns 
                    WHERE table_schema = 'eshopdb' 
                    AND table_name = 'product_images'
                    AND column_name LIKE 'embedding_%'
                    AND data_type = 'USER-DEFINED'
                """)
                )

                cols = [r[0] for r in result.fetchall()]
                expected_cols = [
                    "embedding_efficientnet",
                    "embedding_convnext",
                    "embedding_clip",
                    "embedding_fclip",
                    "embedding_dino",
                ]

                missing = set(expected_cols) - set(cols)
                if missing:
                    print(f"✗ Missing embedding columns: {missing}")
                    print("  Run migrations to add missing columns")
                    return False
                else:
                    print(f"✓ All 5 embedding columns present: {cols}")
            else:
                print("⚠ Database tables not created yet")
                print("  Run: python -m app.database to create tables")

        return True

    except Exception as e:
        print(f"✗ Database connection failed: {e}")
        print("\nMake sure:")
        print("  1. PostgreSQL is running")
        print("  2. DATABASE_URL in .env is correct")
        print("  3. Database 'eshopdb' exists")
        return False


def check_dataset():
    """Check dataset files."""
    print_header("Dataset Check")

    data_dir = Path("data")
    csv_path = data_dir / "styles.csv"
    images_dir = data_dir / "images"

    if not data_dir.exists():
        print(f"✗ 'data/' directory not found")
        print("  Create it and download the dataset")
        return False

    print(f"✓ Data directory exists: {data_dir.absolute()}")

    if csv_path.exists():
        print(f"✓ Metadata file found: {csv_path}")
        try:
            df = pd.read_csv(csv_path, on_bad_lines="skip", nrows=5)
            print(f"  Sample IDs: {df['id'].tolist()}")

            if images_dir.exists():
                print(f"✓ Images directory exists: {images_dir}")

                # Check if sample images exist
                found_count = 0
                for img_id in df["id"].tolist():
                    for ext in [".jpg", ".jpeg", ".png"]:
                        img_path = images_dir / f"{img_id}{ext}"
                        if img_path.exists():
                            found_count += 1
                            break

                if found_count > 0:
                    print(f"✓ Found {found_count}/{len(df)} sample images")
                    return True
                else:
                    print("✗ No matching images found")
                    print("  Check if images are in the correct directory")
                    return False
            else:
                print(f"✗ Images directory not found: {images_dir}")
                return False

        except Exception as e:
            print(f"✗ Error reading CSV: {e}")
            return False
    else:
        print(f"✗ Metadata file not found: {csv_path}")
        print("\nDownload the dataset:")
        print(
            "  python scripts/download_dataset.py --kaggle paramaggarwal/fashion-product-images-small --dest data"
        )
        return False


def check_models():
    """Check if the 5 Champion models can be loaded."""
    print_header("Model Loading Check (5 Champion Models)")

    try:
        from app.model_factory import model_manager
        from app.config import settings

        print(f"Champion models to test: {settings.AVAILABLE_MODELS}")
        results = {}

        for m_name in settings.AVAILABLE_MODELS:
            print(f"\nTesting {m_name}...")
            try:
                model = model_manager.get_embedder(m_name)
                if model:
                    print(f"✓ {m_name} loaded successfully")
                    print(f"  - Dimension: {model.dim}")
                    print(f"  - Device: {model.device}")
                    results[m_name] = True
                else:
                    print(f"✗ {m_name} failed to load (returned None)")
                    results[m_name] = False
            except Exception as e:
                print(f"✗ Error loading {m_name}: {e}")
                results[m_name] = False

        success_count = sum(results.values())
        total_count = len(results)
        print(f"\n{success_count}/{total_count} models loaded successfully")

        return all(results.values())

    except Exception as e:
        print(f"✗ Critical error in model check: {e}")
        import traceback

        traceback.print_exc()
        return False


def check_model_mapping():
    """Check that model mapping utility is working."""
    print_header("Model Mapping Check")

    try:
        from app.model_mapping import (
            get_embedding_prefix,
            get_embedding_column,
            get_model_dimension,
            get_canonical_model_name,
        )

        test_cases = [
            ("efficientnet_b0", "efficientnet", "embedding_efficientnet", 1280),
            ("convnext_tiny", "convnext", "embedding_convnext", 768),
            ("clip_vit_b16", "clip", "embedding_clip", 512),
            ("fashion_clip", "fclip", "embedding_fclip", 512),
            ("dinov2_vits14", "dino", "embedding_dino", 384),
        ]

        all_passed = True
        for model, expected_prefix, expected_col, expected_dim in test_cases:
            prefix = get_embedding_prefix(model)
            col = get_embedding_column(model)
            dim = get_model_dimension(model)

            if (
                prefix == expected_prefix
                and col == expected_col
                and dim == expected_dim
            ):
                print(f"✓ {model}: prefix={prefix}, col={col}, dim={dim}")
            else:
                print(f"✗ {model}: FAILED")
                print(f"  Expected: {expected_prefix}, {expected_col}, {expected_dim}")
                print(f"  Got: {prefix}, {col}, {dim}")
                all_passed = False

        return all_passed

    except Exception as e:
        print(f"✗ Model mapping check failed: {e}")
        import traceback

        traceback.print_exc()
        return False


def main():
    """Run all validation checks."""
    print("\n" + "=" * 70)
    print(" Fashion Image Search System - Setup Validation")
    print(" 5 Champion Models: EfficientNet, ConvNeXt, CLIP, Fashion-CLIP, DINO")
    print("=" * 70)

    # Change to project root
    project_root = Path(__file__).resolve().parent.parent
    os.chdir(project_root)
    print(f"Project root: {project_root}")

    checks = {
        "Environment": check_environment,
        "Model Mapping": check_model_mapping,
        "Database": check_database,
        "Dataset": check_dataset,
        "Models": check_models,
    }

    results = {}
    for name, check_func in checks.items():
        try:
            results[name] = check_func()
        except Exception as e:
            print(f"\n✗ {name} check failed with error: {e}")
            import traceback

            traceback.print_exc()
            results[name] = False

    # Summary
    print_header("Validation Summary")

    all_passed = True
    for name, passed in results.items():
        if passed:
            print(f"✓ {name}: PASSED")
        elif passed is False:
            print(f"✗ {name}: FAILED")
            all_passed = False
        else:
            print(f"⚠ {name}: WARNING")

    print("\n" + "=" * 70)

    if all_passed:
        print("✓ All checks passed! System is ready.")
        print("\nNext steps:")
        print("  1. Load dataset:")
        print("     python -m app.dataset_loader --json data/styles.csv --images data/images --total 1000 --clear")
        print("  2. Start API:")
        print("     uvicorn app.main:app --reload --host 0.0.0.0 --port 8000")
        print("  3. Test API:")
        print("     python scripts/test_api.py")
        return 0
    else:
        print("✗ Some checks failed. Please fix the issues above.")
        return 1


if __name__ == "__main__":
    sys.exit(main())

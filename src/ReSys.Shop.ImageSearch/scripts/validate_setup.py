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

# Add parent directory to path
sys.path.append(str(Path(__file__).resolve().parent.parent))


def print_header(text):
    """Print a formatted section header."""
    print("\n" + "="*70)
    print(f" {text}")
    print("="*70)


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
    import transformers
    print(f"✓ Transformers version: {transformers.__version__}")


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
            result = conn.execute(text("""
                SELECT COUNT(*) 
                FROM information_schema.tables 
                WHERE table_schema = 'public' 
                AND table_name = 'products'
            """))
            if result.scalar() > 0:
                print("✓ Database tables exist")
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
            df = pd.read_csv(csv_path, on_bad_lines='skip', nrows=5)
            print(f"  Sample IDs: {df['id'].tolist()}")
            
            if images_dir.exists():
                print(f"✓ Images directory exists: {images_dir}")
                
                # Check if sample images exist
                found_count = 0
                for img_id in df['id'].tolist():
                    for ext in ['.jpg', '.jpeg', '.png']:
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
        print("  python scripts/download_dataset.py --kaggle paramaggarwal/fashion-product-images-small --dest data")
        return False


def check_models():
    """Check if models can be loaded."""
    print_header("Model Loading Check")
    
    try:
        from app.model_factory import get_embedder
        
        # Test MobileNetV3
        print("Testing MobileNetV3...")
        model = get_embedder('mobilenet_v3')
        if model:
            print(f"✓ MobileNetV3 loaded (dim={model.dim})")
        else:
            print("✗ MobileNetV3 failed to load")
        
        # Test EfficientNet
        print("\nTesting EfficientNet...")
        model = get_embedder('efficientnet_b0')
        if model:
            print(f"✓ EfficientNet loaded (dim={model.dim})")
        else:
            print("✗ EfficientNet failed to load")
        
        # Test CLIP
        print("\nTesting CLIP (may download files)...")
        model = get_embedder('clip')
        if model:
            print(f"✓ CLIP loaded (dim={model.dim})")
        else:
            print("✗ CLIP failed to load")
            print("  This is often due to network issues")
            print("  The system can still work with CNN models only")
        
        return True
        
    except Exception as e:
        print(f"✗ Error loading models: {e}")
        return False


def main():
    """Run all validation checks."""
    print("\n" + "="*70)
    print(" Image Search System - Setup Validation")
    print("="*70)
    
    # Change to project root
    os.chdir(Path(__file__).resolve().parent.parent)
    
    checks = {
        "Environment": check_environment,
        "Database": check_database,
        "Dataset": check_dataset,
        "Models": check_models
    }
    
    results = {}
    for name, check_func in checks.items():
        try:
            results[name] = check_func()
        except Exception as e:
            print(f"\n✗ {name} check failed with error: {e}")
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
    
    print("\n" + "="*70)
    
    if all_passed:
        print("✓ All checks passed! System is ready.")
        print("\nNext steps:")
        print("  1. Load dataset: python -m app.dataset_loader --json data/styles.csv --images data/images --total 4000 --clear")
        print("  2. Start API: uvicorn app.main:app --reload")
        print("  3. Test API: python scripts/test_api.py")
        return 0
    else:
        print("✗ Some checks failed. Please fix the issues above.")
        return 1


if __name__ == "__main__":
    sys.exit(main())
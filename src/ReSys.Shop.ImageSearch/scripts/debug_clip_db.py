import os
import sys
from pathlib import Path
import torch
from sqlalchemy import text
from dotenv import load_dotenv

# Add parent directory to path to import app
sys.path.append(str(Path(__file__).resolve().parent.parent))

load_dotenv()

def debug():
    print("=== Database Connection Debug ===")
    from app.database import engine
    try:
        with engine.connect() as conn:
            result = conn.execute(text("SELECT version();"))
            print(f"PostgreSQL Version: {result.fetchone()[0]}")
            
            # Check for pgvector extension
            try:
                conn.execute(text("CREATE EXTENSION IF NOT EXISTS vector;"))
                conn.commit()
                print("pgvector extension is enabled.")
            except Exception as ve:
                print(f"Warning: Could not ensure pgvector extension: {ve}")

            # Check if product_images table exists and has vector columns
            try:
                result = conn.execute(text("""
                    SELECT column_name, data_type 
                    FROM information_schema.columns 
                    WHERE table_name = 'product_images' AND column_name LIKE 'embedding%';
                """))
                cols = result.fetchall()
                if cols:
                    print(f"Found vector columns in product_images: {[c[0] for c in cols]}")
                else:
                    print("Error: No embedding columns found in product_images table.")
            except Exception as e:
                print(f"Error checking table structure: {e}")
    except Exception as e:
        print(f"CRITICAL: Database connection failed: {e}")

    print("\n=== CLIP Model Debug ===")
    model_id = "openai/clip-vit-base-patch32"
    print(f"Attempting to load CLIP components for: {model_id}")
    
    try:
        from transformers import CLIPImageProcessor, CLIPVisionModel, CLIPProcessor, CLIPModel
        
        print("1. Attempting CLIPImageProcessor.from_pretrained...")
        try:
            processor = CLIPImageProcessor.from_pretrained(model_id)
            print("Successfully loaded CLIPImageProcessor.")
        except Exception as e:
            print(f"FAILED CLIPImageProcessor: {e}")

        print("2. Attempting CLIPProcessor.from_pretrained...")
        try:
            processor = CLIPProcessor.from_pretrained(model_id)
            print("Successfully loaded CLIPProcessor.")
        except Exception as e:
            print(f"FAILED CLIPProcessor: {e}")

        print("3. Attempting CLIPVisionModel.from_pretrained...")
        try:
            # Setting low_cpu_mem_usage=False might help on some systems
            model = CLIPVisionModel.from_pretrained(model_id)
            print("Successfully loaded CLIPVisionModel.")
        except Exception as e:
            print(f"FAILED CLIPVisionModel: {e}")

    except ImportError as e:
        print(f"CRITICAL: transformers library missing or broken: {e}")
    except Exception as e:
        print(f"Unexpected error: {e}")

if __name__ == "__main__":
    debug()

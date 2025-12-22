import os
from pathlib import Path
from huggingface_hub import snapshot_download
from dotenv import load_dotenv

load_dotenv()

def download():
    model_id = "openai/clip-vit-base-patch32"
    token = os.getenv("HF_TOKEN")
    local_dir = Path("models/clip-vit-base-patch32")
    
    print(f"=== CLIP Model Downloader ===")
    print(f"Model ID: {model_id}")
    print(f"Target Directory: {local_dir.absolute()}")
    
    if not token:
        print("Warning: No HF_TOKEN found in .env. Attempting anonymous download...")
    
    try:
        print("Downloading files (this may take a few minutes)...")
        snapshot_download(
            repo_id=model_id,
            local_dir=str(local_dir),
            token=token,
            local_dir_use_symlinks=False
        )
        print("\nSUCCESS: CLIP model downloaded successfully.")
        print(f"You can now set CLIP_MODEL_PATH={local_dir} in your .env file.")
    except Exception as e:
        print(f"\nERROR: {e}")
        print("\nIf you got a 403 Forbidden, please ensure you have a valid HF_TOKEN in your .env file.")

if __name__ == "__main__":
    download()


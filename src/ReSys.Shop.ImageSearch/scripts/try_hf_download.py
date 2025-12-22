import os
import sys
from pathlib import Path
from huggingface_hub import snapshot_download, hf_hub_download
from dotenv import load_dotenv

load_dotenv()

def try_download():
    model_id = "openai/clip-vit-base-patch32"
    token = os.getenv("HF_TOKEN")
    
    print(f"=== Hugging Face Download Test ===")
    print(f"Model: {model_id}")
    print(f"Token present: {bool(token)}")
    
    local_dir = Path("models/clip-vit-base-patch32")
    local_dir.mkdir(parents=True, exist_ok=True)
    
    print(f"\nAttempting to download config.json...")
    try:
        path = hf_hub_download(
            repo_id=model_id,
            filename="config.json",
            token=token,
            local_dir=str(local_dir)
        )
        print(f"Successfully downloaded config.json to: {path}")
        
        print("\nAttempting full snapshot download (excluding large weights for test)...")
        # Just to see if we can get the metadata
        snapshot_download(
            repo_id=model_id,
            token=token,
            local_dir=str(local_dir),
            ignore_patterns=["*.bin", "*.safetensors", "*.msgpack", "*.h5"]
        )
        print("Successfully downloaded model metadata/configs.")
        
        print("\nSUCCESS: Connection to Hugging Face Hub is working.")
        print(f"Files are in: {local_dir.absolute()}")
        
    except Exception as e:
        print(f"\nFAILED: {e}")
        if "403" in str(e):
            print("\nThis is a 403 Forbidden error. Possible reasons:")
            print("1. Your IP is restricted by Hugging Face.")
            print("2. A proxy or firewall is blocking the request.")
            print("3. You need a Hugging Face token (HF_TOKEN) in your .env file.")

if __name__ == "__main__":
    try_download()

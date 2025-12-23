"""Simple downloader + extractor for a dataset ZIP/TAR archive.

Usage examples:
    python scripts/download_dataset.py --url https://example.com/fashion-dataset.zip --dest data/ --metadata styles.json

This script will:
- download the archive to a temporary file
- extract it to the destination directory
- verify that images and a metadata file exist
"""
import argparse
import sys
import os
import requests
import shutil
import tempfile
import zipfile
import tarfile
from pathlib import Path


def download_file(url: str, dest: Path, chunk_size: int = 8192):
    dest.parent.mkdir(parents=True, exist_ok=True)
    with requests.get(url, stream=True, timeout=30) as r:
        r.raise_for_status()
        with open(dest, 'wb') as f:
            for chunk in r.iter_content(chunk_size=chunk_size):
                if chunk:
                    f.write(chunk)
    return dest


def extract_archive(archive_path: Path, dest_dir: Path):
    dest_dir.mkdir(parents=True, exist_ok=True)
    if zipfile.is_zipfile(archive_path):
        with zipfile.ZipFile(archive_path, 'r') as z:
            z.extractall(dest_dir)
        return True
    try:
        if tarfile.is_tarfile(archive_path):
            with tarfile.open(archive_path, 'r:*') as t:
                t.extractall(dest_dir)
            return True
    except Exception:
        pass
    return False


def verify_layout(dest_dir: Path, metadata_name: str) -> bool:
    """Verify that dest_dir contains at least some image files and the metadata file."""
    img_count = 0
    for ext in ('.jpg', '.jpeg', '.png'):
        img_count += len(list(dest_dir.rglob(f'*{ext}')))
    meta_path = dest_dir / metadata_name
    return img_count > 0 and meta_path.exists()


def download_from_kaggle(dataset_id: str, dest_dir: Path):
    try:
        import kaggle
        print(f"Downloading dataset {dataset_id} from Kaggle...")
        dest_dir.mkdir(parents=True, exist_ok=True)
        kaggle.api.dataset_download_files(dataset_id, path=str(dest_dir), unzip=True)
        return True
    except ImportError:
        print("Error: 'kaggle' package not installed. Run 'pip install kaggle'.")
    except Exception as e:
        print(f"Error downloading from Kaggle: {e}")
    return False


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Download and extract a dataset archive')
    parser.add_argument('--url', help='Download URL for the archive (zip/tar)')
    parser.add_argument('--kaggle', help='Kaggle dataset ID (e.g., paramaggarwal/fashion-product-images-small)')
    parser.add_argument('--dest', default='data', help='Destination directory to extract into')
    parser.add_argument('--metadata', default='styles.csv', help='Expected metadata filename inside the archive')
    args = parser.parse_args()

    dest_dir = Path(args.dest)
    
    if args.kaggle:
        ok = download_from_kaggle(args.kaggle, dest_dir)
        if not ok:
            sys.exit(1)
    elif args.url:
        with tempfile.TemporaryDirectory() as tmpdir:
            tmp_file = Path(tmpdir) / 'dataset_archive'
            try:
                print(f"Downloading {args.url} ...")
                download_file(args.url, tmp_file)
                print("Extracting archive...")
                ok = extract_archive(tmp_file, dest_dir)
                if not ok:
                    print("Unsupported archive format or extraction failed")
                    sys.exit(2)
            except Exception as e:
                print(f"Error: {e}")
                sys.exit(1)
    else:
        print("Error: Must provide either --url or --kaggle")
        parser.print_help()
        sys.exit(1)

    print("Verifying dataset layout...")
    if verify_layout(dest_dir, args.metadata):
        print(f"Dataset ready at {dest_dir} (found metadata {args.metadata})")
        print("Next: run the dataset loader to populate the DB:")
        print(f"  python -m app.dataset_loader --json {dest_dir / args.metadata} --images {dest_dir / 'images'} --total 4000")
    else:
        print("Warning: could not verify expected layout (images + metadata). Please inspect the extracted files.")
        sys.exit(3)
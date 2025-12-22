import json
import logging
import pandas as pd
from pathlib import Path
from typing import Dict, List, Tuple, Optional
import random
from tqdm import tqdm
from PIL import Image
import uuid
import re
import os
from datetime import datetime

from sqlalchemy import func
from app.config import settings
from app.database import (
    SessionLocal, Product, ProductImage, StockLocation,
    Taxonomy, Taxon, ProductClassification, OptionType, OptionValue, 
    PropertyType, USE_SQLITE_DEV, engine, Base
)
from app.model_factory import model_manager

# Configure logging
logger = logging.getLogger(__name__)

class FashionDatasetLoader:
    def __init__(self, json_path: str, images_dir: str, total_images: int = 4000):
        self.json_path = Path(json_path)
        self.images_dir = Path(images_dir)
        self.total_images = total_images
        self.processed_dir = Path("data/processed")
        self.processed_dir.mkdir(parents=True, exist_ok=True)
        
        self.taxon_cache = {}
        self.option_value_cache = {}
        self.property_type_cache = {}

    def load_metadata(self) -> pd.DataFrame:
        logger.info(f"Loading metadata from {self.json_path}...")
        try:
            if self.json_path.suffix.lower() == '.csv':
                df = pd.read_csv(self.json_path, on_bad_lines='skip')
            else:
                with open(self.json_path, 'r') as f:
                    try:
                        data = json.load(f)
                        if isinstance(data, dict) and 'data' in data:
                            data = data['data']
                    except json.JSONDecodeError:
                        f.seek(0)
                        data = [json.loads(line) for line in f if line.strip()]
                df = pd.DataFrame(data)
            
            logger.info(f"Loaded {len(df)} raw records.")
            essential_cols = ['articleType', 'baseColour', 'productDisplayName']
            cols_to_check = [c for c in essential_cols if c in df.columns]
            df.dropna(subset=cols_to_check, inplace=True)
            return df
        except Exception as e:
            logger.error(f"Error loading metadata: {e}")
            return pd.DataFrame()

    def process_and_load(self, clear_existing: bool = False):
        from sqlalchemy import text
        
        logger.info("Ensuring database tables exist...")
        if not USE_SQLITE_DEV:
            with engine.connect() as conn:
                conn.execute(text("CREATE EXTENSION IF NOT EXISTS vector"))
                conn.commit()
        Base.metadata.create_all(bind=engine)
        
        if clear_existing:
            logger.info("Clearing existing catalog and inventory data...")
            # Use a faster truncate if possible
            with SessionLocal() as db:
                try:
                    if USE_SQLITE_DEV:
                        for table in ["classification", "stock_items", "prices", "variant_option_values", "product_option_types", "product_property_types", "product_images", "variants", "products", "taxa", "taxonomies", "stock_locations"]:
                            db.execute(text(f"DELETE FROM {table}"))
                    else:
                        db.execute(text("TRUNCATE TABLE classification, stock_items, prices, variant_option_values, product_option_types, product_property_types, product_images, variants, products, taxa, taxonomies, stock_locations RESTART IDENTITY CASCADE;"))
                    db.commit()
                except Exception as e:
                    logger.error(f"Error clearing data: {e}")
                    db.rollback()
        
        with SessionLocal() as db:
            try:
                warehouse = self._ensure_warehouse(db)
                category_taxonomy = self._ensure_taxonomy(db, "Categories")
                
                df = self.load_metadata()
                if df.empty: return

                grouped = df.groupby('articleType')
                valid_items = []
                items_per_cat = max(1, self.total_images // len(grouped))
                
                for group_name, group_df in tqdm(grouped, desc="Sampling Categories"):
                    group_df = group_df.sample(frac=1)
                    added = 0
                    for _, row in group_df.iterrows():
                        if added >= items_per_cat or len(valid_items) >= self.total_images: break
                        img_id = str(row.get('id'))
                        img_path = next((p for ext in ['.jpg', '.png', '.jpeg'] if (p := self.images_dir / f"{img_id}{ext}").exists()), None)
                        if not img_path: continue
                        valid_items.append({**row.to_dict(), 'local_path': str(img_path)})
                        added += 1
                
                random.shuffle(valid_items)
                logger.info(f"Selected {len(valid_items)} balanced items.")
                
                n_total = len(valid_items)
                n_train = int(n_total * 0.7)
                n_val = int(n_total * 0.15)
                
                for idx, item in enumerate(tqdm(valid_items, desc="Importing")):
                    raw_id = str(item.get('id'))
                    exists = db.query(Product).filter(Product.public_metadata['original_id'].astext == raw_id).first()
                    if exists: continue
                        
                    split = 'train' if idx < n_train else ('val' if idx < n_train + n_val else 'test')
                    self._import_product(db, item, warehouse, split, category_taxonomy)
                    if idx > 0 and idx % 100 == 0: db.commit()
                
                db.commit()
                logger.info("Import complete!")
                self.report_metrics(db)
            except Exception as e:
                logger.error(f"Error during import: {e}")
                db.rollback()

    def report_metrics(self, db):
        total = db.query(func.count(Product.id)).scalar()
        logger.info(f"\nTotal products: {total}")
        # Only report on our 4 SOTA models
        report_map = {
            "efficientnet": "EfficientNet-B0 (Production)",
            "convnext": "ConvNeXt-Tiny (Modern CNN)",
            "clip": "CLIP ViT-B/16 (Semantic)",
            "fclip": "Fashion-CLIP (Domain SOTA)"
        }
        for m, label in report_map.items():
            col = getattr(ProductImage, f"embedding_{m}")
            count = db.query(func.count(ProductImage.id)).filter(col != None).scalar()
            logger.info(f"- {label}: {count}/{total}")

    def _ensure_warehouse(self, db):
        wh_name = "default-warehouse"
        wh = db.query(StockLocation).filter(StockLocation.name == wh_name).first()
        if not wh:
            wh = StockLocation(id=uuid.uuid4(), name=wh_name, presentation="Default Warehouse", active=True, is_default=True)
            db.add(wh); db.flush()
        return wh

    def _ensure_taxonomy(self, db, name):
        norm = name.lower().replace(' ','-')
        t = db.query(Taxonomy).filter(Taxonomy.name == norm).first()
        if not t:
            t = Taxonomy(id=uuid.uuid4(), name=norm, presentation=name)
            db.add(t); db.flush()
        return t

    def _ensure_taxon(self, db, tax_id, name, parent_id=None):
        key = f"{parent_id}|{name}"
        if key in self.taxon_cache: return self.taxon_cache[key]
        t = db.query(Taxon).filter(Taxon.taxonomy_id == tax_id, Taxon.name == name, Taxon.parent_id == parent_id).first()
        if not t:
            t = Taxon(id=uuid.uuid4(), taxonomy_id=tax_id, parent_id=parent_id, name=name, presentation=name, permalink=name.lower().replace(' ', '-'))
            db.add(t); db.flush()
        self.taxon_cache[key] = t
        return t

    def _import_product(self, db, item, wh, split, tax):
        raw_id = str(item.get('id'))
        name = item.get('productDisplayName', f"Product {raw_id}")
        p_id = uuid.uuid4()
        product = Product(id=p_id, name=name[:255], presentation=name[:255],
            slug=re.sub(r'[^a-z0-9-]', '', f"{name.lower().replace(' ', '-')}-{raw_id}"[:255]),
            status=1, public_metadata={ "original_id": raw_id, "split": split })
        db.add(product)
        
        m_cat = self._ensure_taxon(db, tax.id, item.get('masterCategory', 'Other'))
        s_cat = self._ensure_taxon(db, tax.id, item.get('subCategory', 'Other'), m_cat.id)
        a_type = self._ensure_taxon(db, tax.id, item.get('articleType', 'Other'), s_cat.id)
        db.add(ProductClassification(id=uuid.uuid4(), product_id=p_id, taxon_id=a_type.id))
        
        local_path = item['local_path']
        
        # Extract features using all available models
        embeddings = {}
        for m_name in settings.AVAILABLE_MODELS:
            embedder = model_manager.get_embedder(m_name)
            if embedder:
                features = embedder.extract_features(local_path)
                if features:
                    embeddings[m_name] = features
                else:
                    logger.warning(f"Failed to extract {m_name} for {local_path}")

        # Calculate a simple checksum for CLIP if generated (or use image hash)
        import hashlib
        clip_checksum = None
        if 'clip_vit_b16' in embeddings:
            clip_checksum = hashlib.sha256(str(embeddings['clip_vit_b16']).encode()).hexdigest()

        db.add(ProductImage(
            id=uuid.uuid4(), product_id=p_id, url=local_path, alt=f"{name} (Search)", type="Search",
            embedding_mobilenet=embeddings.get('mobilenet_v3'),
            embedding_efficientnet=embeddings.get('efficientnet_b0'),
            embedding_resnet=embeddings.get('resnet50'),
            embedding_convnext=embeddings.get('convnext_tiny'),
            embedding_clip=embeddings.get('clip_vit_b16'),
            embedding_fclip=embeddings.get('fashion_clip'),
            embedding_dino=embeddings.get('dino_vit_s16'),
            embedding_mobilenet_model="mobilenet_v3" if 'mobilenet_v3' in embeddings else None,
            embedding_efficientnet_model="efficientnet_b0" if 'efficientnet_b0' in embeddings else None,
            embedding_resnet_model="resnet50" if 'resnet50' in embeddings else None,
            embedding_convnext_model="convnext_tiny" if 'convnext_tiny' in embeddings else None,
            embedding_clip_model="clip_vit_b16" if 'clip_vit_b16' in embeddings else None,
            embedding_clip_checksum=clip_checksum,
            embedding_clip_generated_at=datetime.utcnow() if 'clip_vit_b16' in embeddings else None,
            embedding_fclip_model="fashion_clip" if 'fashion_clip' in embeddings else None,
            embedding_dino_model="dino_vit_s16" if 'dino_vit_s16' in embeddings else None
        ))

if __name__ == "__main__":
    import argparse
    logging.basicConfig(level=logging.INFO)
    parser = argparse.ArgumentParser()
    parser.add_argument('--json', required=True)
    parser.add_argument('--images', required=True)
    parser.add_argument('--total', type=int, default=4000)
    parser.add_argument('--clear', action='store_true')
    args = parser.parse_args()
    loader = FashionDatasetLoader(args.json, args.images, args.total)
    loader.process_and_load(clear_existing=args.clear)

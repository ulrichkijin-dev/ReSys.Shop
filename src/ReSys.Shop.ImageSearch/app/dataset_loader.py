import os
import sys
import json
import pandas as pd
from pathlib import Path
from typing import Dict, List
import random
from tqdm import tqdm
from PIL import Image
import uuid
import re
import requests
import io

from sqlalchemy import select, func
from sqlalchemy.orm.attributes import flag_modified
from database import (
    SessionLocal, Product, ProductVariant, ProductImage, StockItem, StockLocation,
    Taxonomy, Taxon, ProductClassification, OptionType, OptionValue, ProductOptionType,
    VariantOptionValue, PropertyType, ProductPropertyType
)
from model_factory import get_embedder

# Constants
DEFAULT_WAREHOUSE_NAME = "default-warehouse"
SIZES_TO_CREATE = ['S', 'M', 'L', 'XL']

class FashionDatasetLoader:
    def __init__(self, json_path: str, images_dir: str, total_images: int = 4000):
        self.json_path = Path(json_path)
        self.images_dir = Path(images_dir)
        self.total_images = total_images
        self.processed_dir = Path("data/processed")
        self.processed_dir.mkdir(parents=True, exist_ok=True)
        
        # Cache for lookup performance
        self.taxon_cache = {} # key: "parent_id|name", value: Taxon object
        self.option_value_cache = {} # key: "option_type_id|name", value: OptionValue object
        self.property_type_cache = {} # key: "name", value: PropertyType object
        
        self.skipped_items = 0

    def load_metadata(self) -> pd.DataFrame:
        print(f"Loading metadata from {self.json_path}...")
        try:
            with open(self.json_path, 'r') as f:
                try:
                    data = json.load(f)
                    if isinstance(data, dict) and 'data' in data:
                        data = data['data']
                except json.JSONDecodeError:
                    f.seek(0)
                    data = [json.loads(line) for line in f if line.strip()]
            
            df = pd.DataFrame(data)
            print(f"Loaded {len(df)} raw records.")
            # Drop rows with missing essential data for balancing
            df.dropna(subset=['articleType', 'baseColour', 'productDisplayName'], inplace=True)
            return df
        except Exception as e:
            print(f"Error loading JSON: {e}")
            return pd.DataFrame()

    def _validate_item(self, item: Dict) -> (bool, List[str]):
        errors = []
        raw_id = item.get('id')

        if not item.get('productDisplayName'): errors.append(f"Missing 'productDisplayName' for id={raw_id}")
        if not item.get('masterCategory'): errors.append(f"Missing 'masterCategory' for id={raw_id}")
        if not item.get('subCategory'): errors.append(f"Missing 'subCategory' for id={raw_id}")
        if not item.get('articleType'): errors.append(f"Missing 'articleType' for id={raw_id}")
        if not item.get('baseColour'): errors.append(f"Missing 'baseColour' for id={raw_id}")

        return len(errors) == 0 , errors

    def process_and_load(self):
        db = SessionLocal()
        self.skipped_items = 0
        try:
            # 1. Setup Infrastructure
            warehouse = self._ensure_warehouse(db)
            category_taxonomy = self._ensure_taxonomy(db, "Categories")
            color_option_type = self._ensure_option_type(db, "Color", "Color")
            size_option_type = self._ensure_option_type(db, "Size", "Size")
            
            # 2. Load & Balance Data
            df = self.load_metadata()
            if df.empty:
                return

            print("Balancing dataset across 'articleType' and verifying images...")
            grouped = df.groupby('articleType')
            valid_items = []
            
            if len(grouped) == 0:
                print("No categories to group by. Proceeding with simple random sampling.")
                df_sample = df.sample(frac=1, random_state=42).reset_index(drop=True)
                source_iterator = [df_sample]
            else:
                items_per_category = max(1, self.total_images // len(grouped))
                print(f"Aiming for up to {items_per_category} items per category.")
                source_iterator = grouped

            for group_name, group_df in tqdm(source_iterator, desc="Processing categories"):
                group_df = group_df.sample(frac=1)  # Shuffle within the group
                items_added_for_group = 0
                for _, row in group_df.iterrows():
                    if items_added_for_group >= items_per_category and len(grouped) > 1:
                        break
                    if len(valid_items) >= self.total_images:
                        break

                    img_id = str(row.get('id'))
                    img_path = next((p for ext in ['.jpg', '.png', '.jpeg'] if (p := self.images_dir / f"{img_id}{ext}").exists()), None)
                    
                    if not img_path:
                        self.skipped_items += 1
                        continue
                    
                    try:
                        Image.open(img_path).verify()
                        is_valid, _ = self._validate_item(row)
                        if is_valid:
                            valid_items.append({**row.to_dict(), 'local_path': str(img_path)})
                            items_added_for_group += 1
                        else:
                             self.skipped_items += 1
                    except Exception:
                        self.skipped_items += 1
                        continue
            
            random.shuffle(valid_items)
            print(f"Selected {len(valid_items)} valid and balanced items for processing.")
            
            # 3. Models for embedding
            print("Loading models for comparison...")
            embedder_mobilenet = get_embedder('mobilenet_v3')
            embedder_efficientnet = get_embedder('efficientnet_b0')
            embedder_clip = get_embedder('clip')
            
            # 4. Process
            n_total = len(valid_items)
            n_train = int(n_total * 0.7)
            n_val = int(n_total * 0.15)
            
            for idx, item in tqdm(enumerate(valid_items), total=n_total, desc="Importing"):
                split = 'train' if idx < n_train else ('val' if idx < n_train + n_val else 'test')
                
                self._import_product(
                    db, item, warehouse, 
                    embedder_mobilenet, embedder_efficientnet, embedder_clip, 
                    split, category_taxonomy, color_option_type, size_option_type
                )
                
                if idx > 0 and idx % 100 == 0:
                    db.commit()
            
            db.commit()
            print("Import complete!")
            
        except Exception as e:
            print(f"Fatal error: {e}")
            db.rollback()
        finally:
            db.close()

    def report_metrics(self):
        db = SessionLocal()
        try:
            print("\n--- Dataset Metrics ---")

            total_products = db.query(func.count(Product.id)).scalar()
            print(f"Total products imported: {total_products} (skipped: {self.skipped_items})")

            total_variants = db.query(func.count(ProductVariant.id)).scalar()
            print(f"Total variants created: {total_variants}")

            total_images = db.query(func.count(ProductImage.id)).scalar()
            print(f"Total images stored: {total_images}")
            
            print("\n--- Embedding Status ---")
            mobilenet_count = db.query(func.count(ProductImage.id)).filter(ProductImage.embedding_mobilenet != None).scalar()
            efficientnet_count = db.query(func.count(ProductImage.id)).filter(ProductImage.embedding_efficientnet != None).scalar()
            clip_count = db.query(func.count(ProductImage.id)).filter(ProductImage.embedding_clip != None).scalar()
            print(f"Images with MobileNetV3 embeddings: {mobilenet_count}/{total_products}")
            print(f"Images with EfficientNetB0 embeddings: {efficientnet_count}/{total_products}")
            print(f"Images with CLIP embeddings: {clip_count}/{total_products}")

            print("\n--- Data Splits ---")
            train_count = db.query(func.count(Product.id)).filter(Product.public_metadata['split'].astext == 'train').scalar()
            val_count = db.query(func.count(Product.id)).filter(Product.public_metadata['split'].astext == 'val').scalar()
            test_count = db.query(func.count(Product.id)).filter(Product.public_metadata['split'].astext == 'test').scalar()
            print(f"Train / Validation / Test split: {train_count} / {val_count} / {test_count}")
            
            print("\n--- Category Distribution (Top 20 Article Types) ---")
            article_counts = db.query(
                Taxon.presentation, func.count(ProductClassification.product_id)
            ).join(ProductClassification, ProductClassification.taxon_id == Taxon.id)\
             .group_by(Taxon.presentation)\
             .order_by(func.count(ProductClassification.product_id).desc())\
             .limit(20).all()
            
            for name, count in article_counts:
                print(f"- {name}: {count} products")

        except Exception as e:
            print(f"Error generating metrics: {e}")
        finally:
            db.close()

    def _ensure_warehouse(self, db):
        wh = db.query(StockLocation).filter(StockLocation.name == DEFAULT_WAREHOUSE_NAME).first()
        if not wh:
            wh = StockLocation(id=uuid.uuid4(), name=DEFAULT_WAREHOUSE_NAME, presentation="Default Warehouse", active=True, is_default=True, type=1, ship_enabled=True)
            db.add(wh); db.commit()
        return wh

    def _ensure_taxonomy(self, db, name):
        normalized_name = name.lower().replace(' ','-')
        t = db.query(Taxonomy).filter(Taxonomy.name == normalized_name).first()
        if not t:
            t = Taxonomy(id=uuid.uuid4(), name=normalized_name, presentation=name)
            db.add(t); db.commit()
        return t

    def _ensure_taxon(self, db, taxonomy_id, name, parent_id=None):
        cache_key = f"{parent_id}|{name}"
        if cache_key in self.taxon_cache: return self.taxon_cache[cache_key]
        
        t = db.query(Taxon).filter(Taxon.taxonomy_id == taxonomy_id, Taxon.name == name, Taxon.parent_id == parent_id).first()
        if not t:
            t = Taxon(id=uuid.uuid4(), taxonomy_id=taxonomy_id, parent_id=parent_id, name=name, presentation=name, permalink=name.lower().replace(' ', '-'))
            db.add(t); db.flush()
        
        self.taxon_cache[cache_key] = t
        return t

    def _ensure_option_type(self, db, name, presentation):
        ot = db.query(OptionType).filter(OptionType.name == name).first()
        if not ot:
            ot = OptionType(id=uuid.uuid4(), name=name, presentation=presentation)
            db.add(ot); db.commit()
        return ot

    def _ensure_option_value(self, db, option_type_id, name, presentation):
        cache_key = f"{option_type_id}|{name}"
        if cache_key in self.option_value_cache: return self.option_value_cache[cache_key]
        
        ov = db.query(OptionValue).filter(OptionValue.option_type_id == option_type_id, OptionValue.name == name).first()
        if not ov:
            ov = OptionValue(id=uuid.uuid4(), option_type_id=option_type_id, name=name, presentation=presentation)
            db.add(ov); db.flush()
            
        self.option_value_cache[cache_key] = ov
        return ov

    def _ensure_property_type(self, db, name, presentation):
        if name in self.property_type_cache: return self.property_type_cache[name]
        pt = db.query(PropertyType).filter(PropertyType.name == name).first()
        if not pt:
            pt = PropertyType(id=uuid.uuid4(), name=name, presentation=presentation)
            db.add(pt); db.flush()
        self.property_type_cache[name] = pt
        return pt

    def _import_product(self, db, item, warehouse, emb_mob, emb_eff, emb_clip, split, taxonomy, opt_color, opt_size):
        raw_id = str(item.get('id', uuid.uuid4()))
        name = item.get('productDisplayName', f"Product {raw_id}")
        
        # 1. Product
        p_id = uuid.uuid4()
        product = Product(
            id=p_id, name=name[:255], presentation=name[:255],
            slug=re.sub(r'[^a-z0-9-]', '', f"{name.lower().replace(' ', '-')}-{raw_id}"[:255]),
            description=f"{item.get('gender')} {item.get('baseColour')} {item.get('articleType')} for {item.get('usage')} - {item.get('season')}",
            status=1, 
            public_metadata={ "original_id": raw_id, "split": split }
        )
        db.add(product)
        flag_modified(product, "public_metadata")
        
        # 2. Taxonomy / Taxons
        master_cat = self._ensure_taxon(db, taxonomy.id, item.get('masterCategory', 'Other'))
        sub_cat = self._ensure_taxon(db, taxonomy.id, item.get('subCategory', 'Other'), master_cat.id)
        article_type = self._ensure_taxon(db, taxonomy.id, item.get('articleType', 'Other'), sub_cat.id)
        db.add(ProductClassification(id=uuid.uuid4(), product_id=p_id, taxon_id=article_type.id))
        
        # 3. Properties
        prop_season = self._ensure_property_type(db, "Season", "Season")
        db.add(ProductPropertyType(id=uuid.uuid4(), product_id=p_id, property_type_id=prop_season.id, property_type_value=str(item.get('season'))))
        
        # 4. Options & Variants
        db.add(ProductOptionType(id=uuid.uuid4(), product_id=p_id, option_type_id=opt_color.id))
        db.add(ProductOptionType(id=uuid.uuid4(), product_id=p_id, option_type_id=opt_size.id))
        
        # Create Master Variant (no options)
        master_variant = ProductVariant(id=uuid.uuid4(), product_id=p_id, sku=f"SKU-{raw_id}-MASTER", is_master=True, track_inventory=False)
        db.add(master_variant)
        
        # Create Variants for each size
        color_val = self._ensure_option_value(db, opt_color.id, item.get('baseColour', 'None'), item.get('baseColour', 'None'))
        
        for size_str in SIZES_TO_CREATE:
            v_id = uuid.uuid4()
            variant = ProductVariant(id=v_id, product_id=p_id, sku=f"SKU-{raw_id}-{color_val.name}-{size_str}", is_master=False)
            db.add(variant)
            
            size_val = self._ensure_option_value(db, opt_size.id, size_str, size_str)
            db.add(VariantOptionValue(id=uuid.uuid4(), variant_id=v_id, option_value_id=color_val.id))
            db.add(VariantOptionValue(id=uuid.uuid4(), variant_id=v_id, option_value_id=size_val.id))
            
            db.add(StockItem(id=uuid.uuid4(), variant_id=v_id, stock_location_id=warehouse.id, quantity_on_hand=random.randint(5, 100)))

        # 5. Images
        local_path = item['local_path']
        db.add(ProductImage(id=uuid.uuid4(), product_id=p_id, url=local_path, alt=name, type="Default", position=1, content_type="image/jpeg"))
        
        embeddings = {
            "mobilenet": emb_mob.extract_features(local_path),
            "efficientnet": emb_eff.extract_features(local_path),
            "clip": emb_clip.extract_features(local_path)
        }
        
        db.add(ProductImage(
            id=uuid.uuid4(), product_id=p_id, url=local_path, alt=f"{name} (Search)", type="Search", position=2, content_type="image/jpeg",
            embedding_mobilenet=embeddings["mobilenet"],
            embedding_efficientnet=embeddings["efficientnet"],
            embedding_clip=embeddings["clip"],
            embedding_mobilenet_model=emb_mob.name,
            embedding_efficientnet_model=emb_eff.name,
            embedding_clip_model=emb_clip.name,
        ))

if __name__ == "__main__":
    import argparse
    parser = argparse.ArgumentParser()
    parser.add_argument('--json', required=True)
    parser.add_argument('--images', required=True)
    parser.add_argument('--total', type=int, default=4000)
    args = parser.parse_args()
    
    loader = FashionDatasetLoader(args.json, args.images, args.total)
    loader.process_and_load()
    loader.report_metrics()
"""
Fashion Dataset Loader for Image Search System

This script loads fashion product data from a JSON/CSV file and images directory
into the PostgreSQL database with pgvector embeddings for all three models:
- MobileNetV3 (Efficient CNN)
- EfficientNetB0 (Scaled CNN)
- CLIP ViT (Vision Transformer)

The dataset is split into train (70%), validation (15%), and test (15%) sets.
"""

import json
import logging
import pandas as pd
from pathlib import Path
from typing import Dict, List, Tuple, Optional
import random
from tqdm import tqdm
from PIL import Image, UnidentifiedImageError
import uuid
import re

logging.basicConfig(
    level=logging.INFO,
    format='[%(asctime)s] %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

from sqlalchemy import func, text
from sqlalchemy.orm.attributes import flag_modified
from app.database import (
    SessionLocal, Product, ProductVariant, ProductImage, StockItem, StockLocation,
    Taxonomy, Taxon, ProductClassification, OptionType, OptionValue, ProductOptionType,
    VariantOptionValue, PropertyType, ProductPropertyType, engine, Base
)
from app.model_factory import get_embedder

# Constants
DEFAULT_WAREHOUSE_NAME = "default-warehouse"
SIZES_TO_CREATE = ['S', 'M', 'L', 'XL']
TRAIN_SPLIT = 0.70
VAL_SPLIT = 0.15
TEST_SPLIT = 0.15


class FashionDatasetLoader:
    """
    Loads fashion dataset into the database with embeddings from all three models.
    Balances data across categories and creates train/val/test splits.
    """
    
    def __init__(self, json_path: str, images_dir: str, total_images: int = 4000):
        self.json_path = Path(json_path)
        self.images_dir = Path(images_dir)
        self.total_images = total_images
        self.processed_dir = Path("data/processed")
        self.processed_dir.mkdir(parents=True, exist_ok=True)
        
        # Cache for lookup performance
        self.taxon_cache = {}  # key: "parent_id|name", value: Taxon object
        self.option_value_cache = {}  # key: "option_type_id|name", value: OptionValue
        self.property_type_cache = {}  # key: "name", value: PropertyType
        
        self.skipped_items = 0
        self.stats = {
            "total_processed": 0,
            "embeddings_generated": {"mobilenet": 0, "efficientnet": 0, "clip": 0},
            "embeddings_failed": {"mobilenet": 0, "efficientnet": 0, "clip": 0}
        }

    def load_metadata(self) -> pd.DataFrame:
        """Load metadata from JSON or CSV file."""
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
            
            logger.info(f"Loaded {len(df)} raw records")
            
            # Drop rows with missing essential data
            essential_cols = ['articleType', 'baseColour', 'productDisplayName']
            cols_to_check = [c for c in essential_cols if c in df.columns]
            df.dropna(subset=cols_to_check, inplace=True)
            
            logger.info(f"After filtering: {len(df)} valid records")
            return df
            
        except Exception as e:
            logger.error(f"Error loading metadata: {e}")
            return pd.DataFrame()

    def _validate_item(self, item: Dict) -> Tuple[bool, List[str]]:
        """Validate a single item; returns (is_valid, list_of_errors)."""
        errors: List[str] = []
        raw_id = item.get('id')

        required_fields = {
            'productDisplayName': 'Product name',
            'masterCategory': 'Master category',
            'subCategory': 'Sub category',
            'articleType': 'Article type',
            'baseColour': 'Base color'
        }
        
        for field, description in required_fields.items():
            if not item.get(field):
                errors.append(f"Missing '{description}' for id={raw_id}")

        return len(errors) == 0, errors

    def process_and_load(self, clear_existing: bool = False):
        """Main processing pipeline."""
        logger.info("=== Starting Dataset Load Process ===")
        
        # 1. Ensure database tables exist
        logger.info("Ensuring database tables exist...")
        with engine.connect() as conn:
            conn.execute(text("CREATE EXTENSION IF NOT EXISTS vector"))
            conn.commit()
        
        Base.metadata.create_all(bind=engine)
        
        # 2. Clear existing data if requested
        if clear_existing:
            logger.warning("Clearing existing catalog and inventory data...")
            db = SessionLocal()
            try:
                db.execute(text("""
                    TRUNCATE TABLE 
                        classification, stock_items, prices, variant_option_values, 
                        product_option_types, product_property_types, product_images, 
                        variants, products, taxa, taxonomies, stock_locations 
                    RESTART IDENTITY CASCADE
                """))
                db.commit()
                logger.info("Database cleared successfully")
            except Exception as e:
                logger.error(f"Error clearing data: {e}")
                db.rollback()
            finally:
                db.close()
        
        db = SessionLocal()
        self.skipped_items = 0
        
        try:
            # 3. Setup Infrastructure
            logger.info("Setting up infrastructure...")
            warehouse = self._ensure_warehouse(db)
            category_taxonomy = self._ensure_taxonomy(db, "Categories")
            color_option_type = self._ensure_option_type(db, "Color", "Color")
            size_option_type = self._ensure_option_type(db, "Size", "Size")
            
            # 4. Load & Balance Data
            df = self.load_metadata()
            if df.empty:
                logger.error("No data to process")
                return

            logger.info("Balancing dataset across 'articleType' and verifying images...")
            grouped = df.groupby('articleType')
            valid_items = []
            
            if len(grouped) == 0:
                logger.info("No categories to group by. Using simple random sampling.")
                df_sample = df.sample(frac=1, random_state=42).reset_index(drop=True)
                source_iterator = [("all", df_sample)]
            else:
                items_per_category = max(1, self.total_images // len(grouped))
                logger.info(f"Aiming for up to {items_per_category} items per category")
                source_iterator = grouped

            for group_name, group_df in tqdm(source_iterator, desc="Processing categories"):
                group_df = group_df.sample(frac=1)  # Shuffle within group
                items_added_for_group = 0
                
                for _, row in group_df.iterrows():
                    if items_added_for_group >= items_per_category and len(grouped) > 1:
                        break
                    if len(valid_items) >= self.total_images:
                        break

                    img_id = str(row.get('id'))
                    img_path = next(
                        (p for ext in ['.jpg', '.png', '.jpeg'] 
                         if (p := self.images_dir / f"{img_id}{ext}").exists()), 
                        None
                    )
                    
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
            logger.info(f"Selected {len(valid_items)} valid and balanced items")
            
            # 5. Load Models for Embedding
            logger.info("Loading embedding models...")
            embedder_mobilenet = get_embedder('mobilenet_v3')
            embedder_efficientnet = get_embedder('efficientnet_b0')
            embedder_clip = get_embedder('clip')
            
            if not embedder_mobilenet:
                logger.warning("MobileNet failed to load")
            if not embedder_efficientnet:
                logger.warning("EfficientNet failed to load")
            if not embedder_clip:
                logger.warning("CLIP failed to load - will continue without it")
            
            # 6. Process Items with Train/Val/Test Split
            n_total = len(valid_items)
            n_train = int(n_total * TRAIN_SPLIT)
            n_val = int(n_total * VAL_SPLIT)
            
            logger.info(f"Split sizes - Train: {n_train}, Val: {n_val}, Test: {n_total - n_train - n_val}")
            
            for idx, item in enumerate(tqdm(valid_items, desc="Importing products")):
                if idx < n_train:
                    split = 'train'
                elif idx < n_train + n_val:
                    split = 'val'
                else:
                    split = 'test'

                self._import_product(
                    db, item, warehouse,
                    embedder_mobilenet, embedder_efficientnet, embedder_clip,
                    split, category_taxonomy, color_option_type, size_option_type
                )

                # Commit every 100 items
                if (idx + 1) % 100 == 0:
                    db.commit()
                    logger.info(f"Progress: {idx + 1}/{n_total} products imported")
            
            db.commit()
            logger.info("✓ Import complete!")
            
        except Exception as e:
            logger.error(f"Fatal error during processing: {e}", exc_info=True)
            db.rollback()
            raise
        finally:
            db.close()

    def report_metrics(self):
        """Generate and display dataset metrics."""
        db = SessionLocal()
        try:
            logger.info("\n" + "="*70)
            logger.info("DATASET METRICS")
            logger.info("="*70)

            total_products = db.query(func.count(Product.id)).scalar()
            logger.info(f"Total products imported: {total_products} (skipped: {self.skipped_items})")

            total_variants = db.query(func.count(ProductVariant.id)).scalar()
            logger.info(f"Total variants created: {total_variants}")

            total_images = db.query(func.count(ProductImage.id)).scalar()
            logger.info(f"Total images stored: {total_images}")
            
            logger.info("\n--- Embedding Status ---")
            mobilenet_count = db.query(func.count(ProductImage.id)).filter(
                ProductImage.embedding_mobilenet != None
            ).scalar()
            efficientnet_count = db.query(func.count(ProductImage.id)).filter(
                ProductImage.embedding_efficientnet != None
            ).scalar()
            clip_count = db.query(func.count(ProductImage.id)).filter(
                ProductImage.embedding_clip != None
            ).scalar()
            
            logger.info(f"MobileNetV3 embeddings: {mobilenet_count}/{total_images} ({mobilenet_count/total_images*100:.1f}%)")
            logger.info(f"EfficientNet embeddings: {efficientnet_count}/{total_images} ({efficientnet_count/total_images*100:.1f}%)")
            logger.info(f"CLIP embeddings: {clip_count}/{total_images} ({clip_count/total_images*100:.1f}%)")

            logger.info("\n--- Data Splits ---")
            train_count = db.query(func.count(Product.id)).filter(
                Product.public_metadata['split'].astext == 'train'
            ).scalar()
            val_count = db.query(func.count(Product.id)).filter(
                Product.public_metadata['split'].astext == 'val'
            ).scalar()
            test_count = db.query(func.count(Product.id)).filter(
                Product.public_metadata['split'].astext == 'test'
            ).scalar()
            logger.info(f"Train: {train_count} ({train_count/total_products*100:.1f}%)")
            logger.info(f"Validation: {val_count} ({val_count/total_products*100:.1f}%)")
            logger.info(f"Test: {test_count} ({test_count/total_products*100:.1f}%)")
            
            logger.info("\n--- Top 20 Article Types ---")
            article_counts = db.query(
                Taxon.presentation, func.count(ProductClassification.product_id)
            ).join(
                ProductClassification, ProductClassification.taxon_id == Taxon.id
            ).group_by(Taxon.presentation)\
             .order_by(func.count(ProductClassification.product_id).desc())\
             .limit(20).all()
            
            for name, count in article_counts:
                logger.info(f"  {name}: {count} products")

            logger.info("="*70)

        except Exception as e:
            logger.error(f"Error generating metrics: {e}")
        finally:
            db.close()

    # --- Helper Methods ---
    
    def _ensure_warehouse(self, db):
        """Ensure default warehouse exists."""
        wh = db.query(StockLocation).filter(
            StockLocation.name == DEFAULT_WAREHOUSE_NAME
        ).first()
        if not wh:
            wh = StockLocation(
                id=uuid.uuid4(),
                name=DEFAULT_WAREHOUSE_NAME,
                presentation="Default Warehouse",
                active=True,
                is_default=True,
                type=1,
                ship_enabled=True
            )
            db.add(wh)
            db.commit()
        return wh

    def _ensure_taxonomy(self, db, name):
        """Ensure taxonomy exists."""
        normalized_name = name.lower().replace(' ', '-')
        t = db.query(Taxonomy).filter(Taxonomy.name == normalized_name).first()
        if not t:
            t = Taxonomy(id=uuid.uuid4(), name=normalized_name, presentation=name)
            db.add(t)
            db.commit()
        return t

    def _ensure_taxon(self, db, taxonomy_id, name, parent_id=None):
        """Ensure taxon exists (with caching)."""
        cache_key = f"{parent_id}|{name}"
        if cache_key in self.taxon_cache:
            return self.taxon_cache[cache_key]
        
        t = db.query(Taxon).filter(
            Taxon.taxonomy_id == taxonomy_id,
            Taxon.name == name,
            Taxon.parent_id == parent_id
        ).first()
        
        if not t:
            t = Taxon(
                id=uuid.uuid4(),
                taxonomy_id=taxonomy_id,
                parent_id=parent_id,
                name=name,
                presentation=name,
                permalink=name.lower().replace(' ', '-')
            )
            db.add(t)
            db.flush()
        
        self.taxon_cache[cache_key] = t
        return t

    def _ensure_option_type(self, db, name, presentation):
        """Ensure option type exists."""
        ot = db.query(OptionType).filter(OptionType.name == name).first()
        if not ot:
            ot = OptionType(id=uuid.uuid4(), name=name, presentation=presentation)
            db.add(ot)
            db.commit()
        return ot

    def _ensure_option_value(self, db, option_type_id, name, presentation):
        """Ensure option value exists (with caching)."""
        cache_key = f"{option_type_id}|{name}"
        if cache_key in self.option_value_cache:
            return self.option_value_cache[cache_key]
        
        ov = db.query(OptionValue).filter(
            OptionValue.option_type_id == option_type_id,
            OptionValue.name == name
        ).first()
        
        if not ov:
            ov = OptionValue(
                id=uuid.uuid4(),
                option_type_id=option_type_id,
                name=name,
                presentation=presentation
            )
            db.add(ov)
            db.flush()
            
        self.option_value_cache[cache_key] = ov
        return ov

    def _ensure_property_type(self, db, name, presentation):
        """Ensure property type exists (with caching)."""
        if name in self.property_type_cache:
            return self.property_type_cache[name]
            
        pt = db.query(PropertyType).filter(PropertyType.name == name).first()
        if not pt:
            pt = PropertyType(id=uuid.uuid4(), name=name, presentation=presentation)
            db.add(pt)
            db.flush()
            
        self.property_type_cache[name] = pt
        return pt

    def _import_product(
        self, db, item, warehouse, 
        emb_mob, emb_eff, emb_clip,
        split, taxonomy, opt_color, opt_size
    ):
        """Import a single product with all its relationships and embeddings."""
        raw_id = str(item.get('id', uuid.uuid4()))
        name = item.get('productDisplayName', f"Product {raw_id}")
        
        # 1. Create Product
        p_id = uuid.uuid4()
        product = Product(
            id=p_id,
            name=name[:255],
            presentation=name[:255],
            slug=re.sub(r'[^a-z0-9-]', '', f"{name.lower().replace(' ', '-')}-{raw_id}"[:255]),
            description=f"{item.get('gender')} {item.get('baseColour')} {item.get('articleType')} for {item.get('usage')} - {item.get('season')}",
            status=1,
            public_metadata={"original_id": raw_id, "split": split}
        )
        db.add(product)
        flag_modified(product, "public_metadata")
        
        # 2. Create Taxonomy Hierarchy
        master_cat = self._ensure_taxon(db, taxonomy.id, item.get('masterCategory', 'Other'))
        sub_cat = self._ensure_taxon(db, taxonomy.id, item.get('subCategory', 'Other'), master_cat.id)
        article_type = self._ensure_taxon(db, taxonomy.id, item.get('articleType', 'Other'), sub_cat.id)
        db.add(ProductClassification(id=uuid.uuid4(), product_id=p_id, taxon_id=article_type.id))
        
        # 3. Add Properties
        prop_season = self._ensure_property_type(db, "Season", "Season")
        db.add(ProductPropertyType(
            id=uuid.uuid4(),
            product_id=p_id,
            property_type_id=prop_season.id,
            property_type_value=str(item.get('season'))
        ))
        
        # 4. Create Option Types
        db.add(ProductOptionType(id=uuid.uuid4(), product_id=p_id, option_type_id=opt_color.id))
        db.add(ProductOptionType(id=uuid.uuid4(), product_id=p_id, option_type_id=opt_size.id))
        
        # 5. Create Master Variant
        master_variant = ProductVariant(
            id=uuid.uuid4(),
            product_id=p_id,
            sku=f"SKU-{raw_id}-MASTER",
            is_master=True,
            track_inventory=False
        )
        db.add(master_variant)
        
        # 6. Create Size Variants
        color_val = self._ensure_option_value(
            db, opt_color.id,
            item.get('baseColour', 'None'),
            item.get('baseColour', 'None')
        )
        
        for size_str in SIZES_TO_CREATE:
            v_id = uuid.uuid4()
            variant = ProductVariant(
                id=v_id,
                product_id=p_id,
                sku=f"SKU-{raw_id}-{color_val.name}-{size_str}",
                is_master=False
            )
            db.add(variant)
            
            size_val = self._ensure_option_value(db, opt_size.id, size_str, size_str)
            db.add(VariantOptionValue(id=uuid.uuid4(), variant_id=v_id, option_value_id=color_val.id))
            db.add(VariantOptionValue(id=uuid.uuid4(), variant_id=v_id, option_value_id=size_val.id))
            
            # Add stock
            db.add(StockItem(
                id=uuid.uuid4(),
                variant_id=v_id,
                stock_location_id=warehouse.id,
                quantity_on_hand=random.randint(5, 100)
            ))

        # 7. Create Images with Embeddings
        local_path = item['local_path']
        
        # Default image
        db.add(ProductImage(
            id=uuid.uuid4(),
            product_id=p_id,
            url=local_path,
            alt=name,
            type="Default",
            position=1,
            content_type="image/jpeg"
        ))
        
        # Search image with embeddings
        embeddings = {}
        
        # Extract embeddings safely
        for model_name, embedder in [
            ('mobilenet', emb_mob),
            ('efficientnet', emb_eff),
            ('clip', emb_clip)
        ]:
            try:
                if embedder:
                    emb = embedder.extract_features(local_path)
                    if emb:
                        embeddings[model_name] = emb
                        self.stats["embeddings_generated"][model_name] += 1
                    else:
                        embeddings[model_name] = None
                        self.stats["embeddings_failed"][model_name] += 1
                else:
                    embeddings[model_name] = None
            except Exception as ex:
                logger.warning(f"{model_name.capitalize()} extraction failed for {local_path}: {ex}")
                embeddings[model_name] = None
                self.stats["embeddings_failed"][model_name] += 1

        db.add(ProductImage(
            id=uuid.uuid4(),
            product_id=p_id,
            url=local_path,
            alt=f"{name} (Search)",
            type="Search",
            position=2,
            content_type="image/jpeg",
            embedding_mobilenet=embeddings.get("mobilenet"),
            embedding_efficientnet=embeddings.get("efficientnet"),
            embedding_clip=embeddings.get("clip"),
            embedding_mobilenet_model=(emb_mob.name if emb_mob else None),
            embedding_efficientnet_model=(emb_eff.name if emb_eff else None),
            embedding_clip_model=(emb_clip.name if emb_clip else None),
        ))
        
        self.stats["total_processed"] += 1


if __name__ == "__main__":
    import argparse
    
    parser = argparse.ArgumentParser(description="Load fashion dataset into the Image Search DB")
    parser.add_argument('--json', required=True, help='Path to metadata JSON/CSV file')
    parser.add_argument('--images', required=True, help='Path to images directory')
    parser.add_argument('--total', type=int, default=4000, help='Total number of images to import')
    parser.add_argument('--clear', action='store_true', help='Clear existing data before loading')
    parser.add_argument('--log-level', default='INFO', help='Logging level')
    args = parser.parse_args()

    logging.basicConfig(
        level=getattr(logging, args.log_level.upper(), logging.INFO),
        format='[%(asctime)s] %(levelname)s - %(message)s'
    )

    loader = FashionDatasetLoader(args.json, args.images, args.total)
    loader.process_and_load(clear_existing=args.clear)
    loader.report_metrics()
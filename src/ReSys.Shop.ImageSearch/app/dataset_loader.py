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
    SessionLocal,
    Product,
    ProductImage,
    StockLocation,
    Taxonomy,
    Taxon,
    ProductClassification,
    OptionType,
    OptionValue,
    PropertyType,
    USE_SQLITE_DEV,
    engine,
    Base,
)
from app.model_factory import model_manager

# Configure logging
logger = logging.getLogger(__name__)


class FashionDatasetLoader:
    """
    Loads and processes fashion dataset with multi-model embeddings.
    Supports train/val/test splits for thesis evaluation.
    """

    def __init__(self, json_path: str, images_dir: str, total_images: int = 4000):
        self.json_path = Path(json_path)
        self.images_dir = Path(images_dir)
        self.total_images = total_images
        self.processed_dir = Path("data/processed")
        self.processed_dir.mkdir(parents=True, exist_ok=True)

        # Caches for database entities
        self.taxon_cache = {}
        self.option_value_cache = {}
        self.property_type_cache = {}

    def load_metadata(self) -> pd.DataFrame:
        """Load product metadata from JSON or CSV file."""
        logger.info(f"Loading metadata from {self.json_path}...")
        try:
            if self.json_path.suffix.lower() == ".csv":
                df = pd.read_csv(self.json_path, on_bad_lines="skip")
            else:
                with open(self.json_path, "r") as f:
                    try:
                        data = json.load(f)
                        if isinstance(data, dict) and "data" in data:
                            data = data["data"]
                    except json.JSONDecodeError:
                        f.seek(0)
                        data = [json.loads(line) for line in f if line.strip()]
                df = pd.DataFrame(data)

            logger.info(f"Loaded {len(df)} raw records.")

            # Clean data
            essential_cols = ["articleType", "baseColour", "productDisplayName"]
            cols_to_check = [c for c in essential_cols if c in df.columns]
            df.dropna(subset=cols_to_check, inplace=True)

            return df
        except Exception as e:
            logger.error(f"Error loading metadata: {e}")
            return pd.DataFrame()

    def process_and_load(self, clear_existing: bool = False):
        """Main processing pipeline: load data, extract embeddings, import to database."""
        from sqlalchemy import text

        logger.info("Ensuring database tables exist...")
        if not USE_SQLITE_DEV:
            with engine.connect() as conn:
                conn.execute(text("CREATE EXTENSION IF NOT EXISTS vector"))
                conn.commit()
        Base.metadata.create_all(bind=engine)

        if clear_existing:
            logger.info("Clearing existing catalog and inventory data...")
            with SessionLocal() as db:
                try:
                    if USE_SQLITE_DEV:
                        tables = [
                            "classification",
                            "stock_items",
                            "prices",
                            "variant_option_values",
                            "product_option_types",
                            "product_property_types",
                            "product_images",
                            "variants",
                            "products",
                            "taxa",
                            "taxonomies",
                            "stock_locations",
                        ]
                        for table in tables:
                            db.execute(text(f"DELETE FROM {table}"))
                    else:
                        db.execute(
                            text("""
                            TRUNCATE TABLE eshopdb.classification, 
                                eshopdb.stock_items, eshopdb.prices, 
                                eshopdb.variant_option_values, 
                                eshopdb.product_option_types, 
                                eshopdb.product_property_types, 
                                eshopdb.product_images, eshopdb.variants, 
                                eshopdb.products, eshopdb.taxa, 
                                eshopdb.taxonomies, eshopdb.stock_locations 
                            RESTART IDENTITY CASCADE;
                        """)
                        )
                    db.commit()
                    logger.info("Database cleared successfully")
                except Exception as e:
                    logger.error(f"Error clearing data: {e}")
                    db.rollback()

        with SessionLocal() as db:
            try:
                # Ensure required entities exist
                warehouse = self._ensure_warehouse(db)
                category_taxonomy = self._ensure_taxonomy(db, "Categories")

                # Load and sample dataset
                df = self.load_metadata()
                if df.empty:
                    logger.error("No data loaded, aborting.")
                    return

                # Balanced sampling by category
                grouped = df.groupby("articleType")
                valid_items = []
                items_per_cat = max(1, self.total_images // len(grouped))

                logger.info(f"Sampling {items_per_cat} items per category...")
                for group_name, group_df in tqdm(grouped, desc="Sampling Categories"):
                    group_df = group_df.sample(frac=1)  # Shuffle
                    added = 0
                    for _, row in group_df.iterrows():
                        if (
                            added >= items_per_cat
                            or len(valid_items) >= self.total_images
                        ):
                            break

                        # Check if image exists
                        img_id = str(row.get("id"))
                        img_path = next(
                            (
                                p
                                for ext in [".jpg", ".png", ".jpeg"]
                                if (p := self.images_dir / f"{img_id}{ext}").exists()
                            ),
                            None,
                        )

                        if not img_path:
                            continue

                        valid_items.append(
                            {**row.to_dict(), "local_path": str(img_path)}
                        )
                        added += 1

                random.shuffle(valid_items)
                logger.info(f"Selected {len(valid_items)} balanced items for import.")

                # Create train/val/test splits (70/15/15)
                n_total = len(valid_items)
                n_train = int(n_total * 0.7)
                n_val = int(n_total * 0.15)

                # Import products with embeddings
                for idx, item in enumerate(
                    tqdm(valid_items, desc="Importing Products")
                ):
                    raw_id = str(item.get("id"))

                    # Skip if already exists
                    exists = (
                        db.query(Product)
                        .filter(Product.public_metadata["original_id"].astext == raw_id)
                        .first()
                    )
                    if exists:
                        continue

                    # Determine split
                    if idx < n_train:
                        split = "train"
                    elif idx < n_train + n_val:
                        split = "val"
                    else:
                        split = "test"

                    # Import product with embeddings
                    self._import_product(db, item, warehouse, split, category_taxonomy)

                    # Commit periodically
                    if idx > 0 and idx % 100 == 0:
                        db.commit()
                        logger.info(f"Committed batch at {idx} products")

                db.commit()
                logger.info("✅ Import complete!")
                self.report_metrics(db)

            except Exception as e:
                logger.error(f"Error during import: {e}", exc_info=True)
                db.rollback()
                raise

    def report_metrics(self, db):
        """Report embedding coverage statistics."""
        total = db.query(func.count(Product.id)).scalar()
        logger.info(f"\n{'=' * 60}")
        logger.info(f"IMPORT SUMMARY")
        logger.info(f"{'=' * 60}")
        logger.info(f"Total products: {total}")
        logger.info(f"\nEmbedding Coverage (5 Champion Models):")

        # Report on our 5 champion models (matching C# domain)
        report_map = {
            "efficientnet": "EfficientNet-B0 (Production CNN)",
            "convnext": "ConvNeXt-Tiny (Modern CNN)",
            "clip": "CLIP ViT-B/16 (Semantic Transformer)",
            "fclip": "Fashion-CLIP (Domain-Specific)",
            "dino": "DINOv2 ViT-S/14 (Visual Structure)",
        }

        for prefix, label in report_map.items():
            col = getattr(ProductImage, f"embedding_{prefix}")
            count = db.query(func.count(ProductImage.id)).filter(col != None).scalar()
            coverage = (count / total * 100) if total > 0 else 0
            logger.info(f"  - {label}: {count}/{total} ({coverage:.1f}%)")

        logger.info(f"{'=' * 60}\n")

    def _ensure_warehouse(self, db) -> StockLocation:
        """Ensure default warehouse exists."""
        wh_name = "default-warehouse"
        wh = db.query(StockLocation).filter(StockLocation.name == wh_name).first()
        if not wh:
            wh = StockLocation(
                id=uuid.uuid4(),
                name=wh_name,
                presentation="Default Warehouse",
                type="Warehouse",
                active=True,
                is_default=True,
            )
            db.add(wh)
            db.flush()
        return wh

    def _ensure_taxonomy(self, db, name: str) -> Taxonomy:
        """Ensure taxonomy exists."""
        norm = name.lower().replace(" ", "-")
        t = db.query(Taxonomy).filter(Taxonomy.name == norm).first()
        if not t:
            t = Taxonomy(id=uuid.uuid4(), name=norm, presentation=name)
            db.add(t)
            db.flush()
        return t

    def _ensure_taxon(self, db, tax_id, name: str, parent_id=None) -> Taxon:
        """Ensure taxon (category) exists."""
        key = f"{parent_id}|{name}"
        if key in self.taxon_cache:
            return self.taxon_cache[key]

        t = (
            db.query(Taxon)
            .filter(
                Taxon.taxonomy_id == tax_id,
                Taxon.name == name,
                Taxon.parent_id == parent_id,
            )
            .first()
        )

        if not t:
            t = Taxon(
                id=uuid.uuid4(),
                taxonomy_id=tax_id,
                parent_id=parent_id,
                name=name,
                presentation=name,
                permalink=name.lower().replace(" ", "-"),
            )
            db.add(t)
            db.flush()

        self.taxon_cache[key] = t
        return t

    def _import_product(
        self, db, item: dict, wh: StockLocation, split: str, tax: Taxonomy
    ):
        """Import a single product with all 5 champion model embeddings."""
        raw_id = str(item.get("id"))
        name = item.get("productDisplayName", f"Product {raw_id}")
        p_id = uuid.uuid4()

        # Create product
        product = Product(
            id=p_id,
            name=name[:255],
            presentation=name[:255],
            slug=re.sub(
                r"[^a-z0-9-]", "", f"{name.lower().replace(' ', '-')}-{raw_id}"[:255]
            ),
            status=1,
            public_metadata={"original_id": raw_id, "split": split},
        )
        db.add(product)

        # Create hierarchical categories
        m_cat = self._ensure_taxon(db, tax.id, item.get("masterCategory", "Other"))
        s_cat = self._ensure_taxon(
            db, tax.id, item.get("subCategory", "Other"), m_cat.id
        )
        a_type = self._ensure_taxon(
            db, tax.id, item.get("articleType", "Other"), s_cat.id
        )

        # Link product to category
        db.add(
            ProductClassification(id=uuid.uuid4(), product_id=p_id, taxon_id=a_type.id)
        )

        # Extract embeddings from all 5 champion models
        local_path = item["local_path"]
        embeddings = {}

        logger.debug(f"Extracting embeddings for {name}...")
        for m_name in settings.AVAILABLE_MODELS:
            embedder = model_manager.get_embedder(m_name)
            if embedder:
                features = embedder.extract_features(local_path)
                if features:
                    embeddings[m_name] = features
                else:
                    logger.warning(f"Failed to extract {m_name} for {local_path}")

        import hashlib
        from datetime import datetime, UTC

        def get_metadata(m_name: str):
            """Helper to get embedding metadata."""
            if m_name not in embeddings:
                return None, None, None
            emb = embeddings[m_name]
            checksum = hashlib.sha256(str(emb).encode()).hexdigest()
            return emb, datetime.now(UTC), checksum

        # Prepare all model data using canonical model names
        emb_eff, gen_eff, chk_eff = get_metadata("efficientnet_b0")
        emb_cnxt, gen_cnxt, chk_cnxt = get_metadata("convnext_tiny")
        emb_clip, gen_clip, chk_clip = get_metadata("clip_vit_b16")
        emb_fclip, gen_fclip, chk_fclip = get_metadata("fashion_clip")
        emb_dino, gen_dino, chk_dino = get_metadata("dinov2_vits14")

        # 1. Insert Default Display Image (No embeddings - for UI display only)
        db.add(
            ProductImage(
                id=uuid.uuid4(),
                product_id=p_id,
                url=local_path,
                alt=name,
                type="Default",
                position=0,
            )
        )

        # 2. Insert Search Image (With all 5 Champion Embeddings)
        db.add(
            ProductImage(
                id=uuid.uuid4(),
                product_id=p_id,
                url=local_path,
                alt=f"{name} (Search)",
                type="Search",
                position=1,
                # EfficientNet-B0 (Production CNN)
                embedding_efficientnet=emb_eff,
                embedding_efficientnet_model="efficientnet_b0" if emb_eff else None,
                embedding_efficientnet_generated_at=gen_eff,
                embedding_efficientnet_checksum=chk_eff,
                # ConvNeXt-Tiny (Modern CNN)
                embedding_convnext=emb_cnxt,
                embedding_convnext_model="convnext_tiny" if emb_cnxt else None,
                embedding_convnext_generated_at=gen_cnxt,
                embedding_convnext_checksum=chk_cnxt,
                # CLIP ViT-B/16 (General Semantic)
                embedding_clip=emb_clip,
                embedding_clip_model="clip_vit_b16" if emb_clip else None,
                embedding_clip_generated_at=gen_clip,
                embedding_clip_checksum=chk_clip,
                # Fashion-CLIP (Domain-Specific Semantic)
                embedding_fclip=emb_fclip,
                embedding_fclip_model="fashion_clip" if emb_fclip else None,
                embedding_fclip_generated_at=gen_fclip,
                embedding_fclip_checksum=chk_fclip,
                # DINOv2 ViT-S/14 (Visual Structure)
                embedding_dino=emb_dino,
                embedding_dino_model="dinov2_vits14" if emb_dino else None,
                embedding_dino_generated_at=gen_dino,
                embedding_dino_checksum=chk_dino,
            )
        )


if __name__ == "__main__":
    import argparse

    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s - %(name)s - %(levelname)s - %(message)s",
    )

    parser = argparse.ArgumentParser(description="Load Fashion Dataset")
    parser.add_argument("--json", required=True, help="Path to JSON/CSV metadata")
    parser.add_argument("--images", required=True, help="Path to images directory")
    parser.add_argument(
        "--total", type=int, default=4000, help="Total images to import"
    )
    parser.add_argument("--clear", action="store_true", help="Clear existing data")

    args = parser.parse_args()

    loader = FashionDatasetLoader(args.json, args.images, args.total)
    loader.process_and_load(clear_existing=args.clear)

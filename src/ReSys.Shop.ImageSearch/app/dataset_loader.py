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
from datetime import datetime, timezone

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
    VariantOptionValue,
    PropertyType,
    ProductPropertyType,
    ProductOptionType,
    ProductVariant,
    StockItem,
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
        self.property_type_cache = {}
        self.option_type_cache = {}
        self.option_value_cache = {}

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
        Base.metadata.create_all(bind=engine)

        if clear_existing:
            logger.info("Clearing existing catalog and inventory data...")
            with SessionLocal() as db:
                try:
                    db.execute(
                        text("""
                        TRUNCATE TABLE 
                            eshopdb.prices,
                            eshopdb.variant_option_values,
                            eshopdb.product_option_types,
                            eshopdb.product_property_types,
                            eshopdb.property_types,
                            eshopdb.option_values,
                            eshopdb.option_types,
                            eshopdb.classification, 
                            eshopdb.stock_items, 
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
                warehouses = [
                    self._ensure_warehouse(db, "default-warehouse", "Default Warehouse"),
                    self._ensure_warehouse(db, "secondary-warehouse", "Secondary Warehouse"),
                ]
                
                # Each taxonomy now returns (Taxonomy, RootTaxon)
                cat_tax, cat_root = self._ensure_taxonomy(db, "Categories")
                art_tax, art_root = self._ensure_taxonomy(db, "Article Types")
                gender_tax, gender_root = self._ensure_taxonomy(db, "Genders")
                
                # Ensure property types exist
                prop_gender = self._ensure_property_type(db, "Gender")
                prop_usage = self._ensure_property_type(db, "Usage")
                prop_display_name = self._ensure_property_type(db, "DisplayName")
                prop_season = self._ensure_property_type(db, "Season")
                
                # Ensure option types exist
                opt_color = self._ensure_option_type(db, "Color")
                opt_size = self._ensure_option_type(db, "Size")

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
                    # Determine split
                    if idx < n_train:
                        split = "train"
                    elif idx < n_train + n_val:
                        split = "val"
                    else:
                        split = "test"

                    # Import product with embeddings
                    self._import_product(
                        db, item, warehouses, split, 
                        (cat_tax, cat_root), 
                        (art_tax, art_root), 
                        (gender_tax, gender_root)
                    )

                    # Commit periodically
                    if idx > 0 and idx % 50 == 0:
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
        logger.info(f"{ '=' * 60}")
        logger.info(f"Total products: {total}")
        logger.info(f"\nEmbedding Coverage (5 Champion Models):")

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

        logger.info(f"{ '=' * 60}\n")

    def _ensure_warehouse(self, db, name: str, presentation: str) -> StockLocation:
        """Ensure warehouse exists."""
        wh = db.query(StockLocation).filter(StockLocation.name == name).first()
        if not wh:
            wh = StockLocation(
                id=uuid.uuid4(),
                name=name,
                presentation=presentation,
                type=1,  # 1 = Warehouse
                active=True,
                is_default=(name == "default-warehouse"),
                is_deleted=False,
                version=0,
            )
            db.add(wh)
            db.flush()
        return wh

    def _ensure_taxonomy(self, db, name: str) -> Tuple[Taxonomy, Taxon]:
        """Ensure taxonomy exists and has a root taxon with the same name."""
        norm = name.lower().replace(" ", "-")
        t = db.query(Taxonomy).filter(Taxonomy.name == norm).first()
        if not t:
            t = Taxonomy(id=uuid.uuid4(), name=norm, presentation=name, version=0)
            db.add(t)
            db.flush()
        
        # Ensure root taxon exists with same name as taxonomy
        root = self._ensure_taxon(db, t.id, name)
        return t, root

    def _ensure_taxon(self, db, tax_id, name: str, parent_id=None) -> Taxon:
        """Ensure taxon (category) exists within a specific taxonomy."""
        # Get taxonomy name for unique naming
        taxonomy = db.query(Taxonomy).filter(Taxonomy.id == tax_id).first()
        tax_name = taxonomy.name if taxonomy else "unknown"
        
        # Use a unique internal name to satisfy DB unique constraint: "Taxonomy: Name"
        # but keep presentation and pretty_name as the original Name.
        unique_internal_name = f"{tax_name}: {name}"
        
        # Check cache first
        if unique_internal_name in self.taxon_cache:
            return self.taxon_cache[unique_internal_name]

        # Query by the unique internal name
        t = (
            db.query(Taxon)
            .filter(Taxon.name == unique_internal_name)
            .first()
        )

        if not t:
            # If not found by unique name, check if it exists by plain name 
            # (to handle root taxons or existing data correctly)
            t_plain = db.query(Taxon).filter(Taxon.name == name, Taxon.taxonomy_id == tax_id).first()
            if t_plain:
                t = t_plain
            else:
                t = Taxon(
                    id=uuid.uuid4(),
                    taxonomy_id=tax_id,
                    parent_id=parent_id,
                    name=unique_internal_name if parent_id else name, # Root taxons keep plain name
                    presentation=name,
                    permalink=f"{name.lower().replace(' ', '-')}-{str(uuid.uuid4())[:8]}",
                    pretty_name=name,
                    hide_from_nav=False,
                    automatic=False,
                    rules_match_policy="all",
                    sort_order="manual",
                    marked_for_regenerate_taxon_products=False,
                    version=0
                )
                db.add(t)
                db.flush()

        self.taxon_cache[unique_internal_name] = t
        return t

    def _ensure_property_type(self, db, name: str) -> PropertyType:
        if name in self.property_type_cache:
            return self.property_type_cache[name]
        p = db.query(PropertyType).filter(PropertyType.name == name).first()
        if not p:
            p = PropertyType(
                id=uuid.uuid4(),
                name=name,
                presentation=name,
                kind="String",
                filterable=True,
                display_on="ProductDetails",
                version=0
            )
            db.add(p)
            db.flush()
        self.property_type_cache[name] = p
        return p

    def _ensure_option_type(self, db, name: str) -> OptionType:
        if name in self.option_type_cache:
            return self.option_type_cache[name]
        o = db.query(OptionType).filter(OptionType.name == name).first()
        if not o:
            o = OptionType(
                id=uuid.uuid4(), name=name, presentation=name, filterable=True
            )
            db.add(o)
            db.flush()
        self.option_type_cache[name] = o
        return o

    def _ensure_option_value(self, db, opt_type_id, name: str) -> OptionValue:
        key = f"{opt_type_id}|{name}"
        if key in self.option_value_cache:
            return self.option_value_cache[key]
        ov = (
            db.query(OptionValue)
            .filter(OptionValue.option_type_id == opt_type_id, OptionValue.name == name)
            .first()
        )
        if not ov:
            ov = OptionValue(
                id=uuid.uuid4(),
                option_type_id=opt_type_id,
                name=name,
                presentation=name,
                version=0,
            )
            db.add(ov)
            db.flush()
        self.option_value_cache[key] = ov
        return ov

    def _import_product(
        self,
        db,
        item: dict,
        warehouses: List[StockLocation],
        split: str,
        cat_info: Tuple[Taxonomy, Taxon],
        art_info: Tuple[Taxonomy, Taxon],
        gender_info: Tuple[Taxonomy, Taxon],
    ):
        """Import a single product with metadata, variants, and embeddings."""
        from app.database import Price
        cat_tax, cat_root = cat_info
        art_tax, art_root = art_info
        gender_tax, gender_root = gender_info

        raw_id = str(item.get("id"))
        full_name = item.get("productDisplayName", f"Product {raw_id}")
        name_100 = full_name[:100]
        
        # 1. Check if record with this original_id already exists
        existing_variant = db.query(ProductVariant).filter(ProductVariant.sku == f"SKU-{raw_id}-M").first()
        if existing_variant:
            return

        # 2. Check if product with this name already exists
        product = db.query(Product).filter(Product.name == name_100).first()
        
        if not product:
            p_id = uuid.uuid4()
            product = Product(
                id=p_id,
                name=name_100,
                presentation=full_name,
                slug=re.sub(r"[^a-z0-9-]", "", f"{full_name.lower().replace(' ', '-')}-{raw_id}"[:200]),
                status="Active",
                is_digital=False,
                marked_for_regenerate_taxon_products=False,
                is_deleted=False,
                public_metadata={"original_id": raw_id, "split": split},
                version=0
            )
            db.add(product)

            # --- Taxonomies ---
            # Category Hierarchy (Root -> Master -> Sub -> ArticleType)
            m_cat = self._ensure_taxon(db, cat_tax.id, item.get("masterCategory", "Other"), cat_root.id)
            s_cat = self._ensure_taxon(db, cat_tax.id, item.get("subCategory", "Other"), m_cat.id)
            a_type_cat = self._ensure_taxon(db, cat_tax.id, item.get("articleType", "Other"), s_cat.id)
            
            # Article Type Taxonomy (Root -> ArticleType)
            art_taxon = self._ensure_taxon(db, art_tax.id, item.get("articleType", "Other"), art_root.id)
            
            # Gender Taxonomy (Root -> Gender)
            gender_taxon = self._ensure_taxon(db, gender_tax.id, item.get("gender", "Other"), gender_root.id)

            for t_obj in [a_type_cat, art_taxon, gender_taxon]:
                exists = db.query(ProductClassification).filter(
                    ProductClassification.product_id == p_id,
                    ProductClassification.taxon_id == t_obj.id
                ).first()
                if not exists:
                    db.add(ProductClassification(id=uuid.uuid4(), product_id=p_id, taxon_id=t_obj.id))
                    db.flush()

            # --- Properties ---
            props = {
                "Gender": item.get("gender"),
                "Usage": item.get("usage"),
                "DisplayName": full_name,
                "Season": item.get("season")
            }
            for p_name, p_val in props.items():
                if p_val:
                    pt = self._ensure_property_type(db, p_name)
                    db.add(ProductPropertyType(
                        id=uuid.uuid4(), product_id=p_id, property_type_id=pt.id, 
                        property_type_value=str(p_val), position=0
                    ))
            
            # --- Option Types ---
            for opt_name in ["Color", "Size"]:
                ot = self._ensure_option_type(db, opt_name)
                exists = db.query(ProductOptionType).filter(
                    ProductOptionType.product_id == p_id,
                    ProductOptionType.option_type_id == ot.id
                ).first()
                if not exists:
                    db.add(ProductOptionType(id=uuid.uuid4(), product_id=p_id, option_type_id=ot.id))
        else:
            p_id = product.id

        # --- Variants ---
        # 1 master and 2-3 others (total 3-4 variants)
        num_others = random.randint(2, 3)
        sizes = ["S", "M", "L", "XL", "XXL", "38", "40", "42", "ONESIZE"]
        colors = ["Red", "Blue", "Black", "White", "Green", "Yellow", "Pink", "Purple", "Brown", "Grey"]
        
        opt_color = self._ensure_option_type(db, "Color")
        opt_size = self._ensure_option_type(db, "Size")

        for i in range(num_others + 1):
            is_master = (i == 0)
            v_id = uuid.uuid4()
            sku = f"SKU-{raw_id}-{'M' if is_master else i}"
            
            # Base cost and price in USD with 4 decimal precision
            cost = random.uniform(10.0, 50.0)
            amount = cost * random.uniform(1.5, 3.0)

            variant = ProductVariant(
                id=v_id, product_id=p_id, is_master=is_master, sku=sku,
                track_inventory=True, position=i+1, is_deleted=False, version=0,
                cost_price=round(cost, 4), cost_currency="USD"
            )
            db.add(variant)
            
            # Add Price
            db.add(Price(
                id=uuid.uuid4(), variant_id=v_id, 
                amount=round(amount, 4), currency="USD", version=0
            ))
            
            # Add Option Values to Variant
            v_color = item.get("baseColour") if is_master else random.choice(colors)
            v_size = "M" if is_master else random.choice(sizes)
            
            val_color = self._ensure_option_value(db, opt_color.id, v_color)
            val_size = self._ensure_option_value(db, opt_size.id, v_size)
            
            db.add(VariantOptionValue(id=uuid.uuid4(), variant_id=v_id, option_value_id=val_color.id))
            db.add(VariantOptionValue(id=uuid.uuid4(), variant_id=v_id, option_value_id=val_size.id))
            
            # --- Stock Items ---
            for wh in warehouses:
                db.add(StockItem(
                    id=uuid.uuid4(), variant_id=v_id, stock_location_id=wh.id,
                    sku=sku, quantity_on_hand=random.randint(5, 50),
                    backorderable=True, max_backorder_quantity=0, version=0
                ))

            # --- Embeddings & Images ---
            if is_master:
                local_path = item["local_path"]
                embeddings = {}
                for m_name in settings.AVAILABLE_MODELS:
                    embedder = model_manager.get_embedder(m_name)
                    if embedder:
                        features = embedder.extract_features(local_path)
                        if features: embeddings[m_name] = features

                import hashlib
                def get_metadata(m_name: str):
                    if m_name not in embeddings: return None, None, None
                    emb = embeddings[m_name]
                    checksum = hashlib.sha256(str(emb).encode()).hexdigest()
                    return emb, datetime.now(timezone.utc), checksum

                emb_eff, gen_eff, chk_eff = get_metadata("efficientnet_b0")
                emb_cnxt, gen_cnxt, chk_cnxt = get_metadata("convnext_tiny")
                emb_clip, gen_clip, chk_clip = get_metadata("clip_vit_b16")
                emb_fclip, gen_fclip, chk_fclip = get_metadata("fashion_clip")
                emb_dino, gen_dino, chk_dino = get_metadata("dinov2_vits14")

                # 1. Search Image (with embeddings)
                db.add(ProductImage(
                    id=uuid.uuid4(), product_id=p_id, variant_id=v_id,
                    url=local_path, alt=f"{full_name} (Search)",
                    type="Search", position=1, content_type="image/jpeg",
                    embedding_efficientnet=emb_eff, embedding_efficientnet_model="efficientnet_b0" if emb_eff else None,
                    embedding_efficientnet_generated_at=gen_eff, embedding_efficientnet_checksum=chk_eff,
                    embedding_convnext=emb_cnxt, embedding_convnext_model="convnext_tiny" if emb_cnxt else None,
                    embedding_convnext_generated_at=gen_cnxt, embedding_convnext_checksum=chk_cnxt,
                    embedding_clip=emb_clip, embedding_clip_model="clip_vit_b16" if emb_clip else None,
                    embedding_clip_generated_at=gen_clip, embedding_clip_checksum=chk_clip,
                    embedding_fclip=emb_fclip, embedding_fclip_model="fashion_clip" if emb_fclip else None,
                    embedding_fclip_generated_at=gen_fclip, embedding_fclip_checksum=chk_fclip,
                    embedding_dino=emb_dino, embedding_dino_model="dinov2_vits14" if emb_dino else None,
                    embedding_dino_generated_at=gen_dino, embedding_dino_checksum=chk_dino,
                ))

                # 2. Default Image (Product-level as requested)
                db.add(ProductImage(
                    id=uuid.uuid4(), product_id=p_id, variant_id=None,
                    url=local_path, alt=f"{full_name} (Default)",
                    type="Default", position=0, content_type="image/jpeg"
                ))


if __name__ == "__main__":
    import argparse
    logging.basicConfig(level=logging.INFO, format="%(asctime)s - %(name)s - %(levelname)s - %(message)s")
    parser = argparse.ArgumentParser(description="Load Fashion Dataset")
    parser.add_argument("--json", required=True, help="Path to JSON/CSV metadata")
    parser.add_argument("--images", required=True, help="Path to images directory")
    parser.get_metadata = lambda: None # placeholder
    parser.add_argument("--total", type=int, default=4000, help="Total images to import")
    parser.add_argument("--clear", action="store_true", help="Clear existing data")
    args = parser.parse_args()
    loader = FashionDatasetLoader(args.json, args.images, args.total)
    loader.process_and_load(clear_existing=args.clear)
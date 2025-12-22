"""
Database Module for Fashion Image Search Thesis
================================================

SQLAlchemy ORM models for e-commerce fashion database with multi-model embeddings.

Architecture Support:
- PostgreSQL with pgvector (production)
- SQLite with JSON fallback (development)

Embedding Models (Thesis Comparison):
- MobileNetV3-Small (576-dim): Efficient CNN
- EfficientNet-B0 (1280-dim): Scaled CNN  
- CLIP ViT-B/16 (512-dim): Vision Transformer
- DINO ViT-S/16 (384-dim): Self-supervised Transformer

Author: [Your Name]
Thesis: Building a Fashion E-commerce Application with Recommendation and Image-based Product Search
Date: December 2025
"""

import uuid
import logging
from datetime import datetime, UTC
from typing import Generator
from sqlalchemy import (DECIMAL, Boolean, Column, DateTime, ForeignKey,
                        Integer, String, Text, create_engine, text)
from sqlalchemy.dialects.postgresql import JSONB, UUID
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import relationship, sessionmaker, Session
from pgvector.sqlalchemy import Vector

from app.config import settings

# Configure logging
logger = logging.getLogger(__name__)

# Database Configuration
DATABASE_URL = settings.DATABASE_URL or "postgresql://postgres:12345678@localhost:5432/eshopdb"
USE_SQLITE_DEV = DATABASE_URL.startswith("sqlite")

if USE_SQLITE_DEV:
    logger.info(f"🔧 Database: SQLite at {DATABASE_URL}")
    engine = create_engine(DATABASE_URL, connect_args={"check_same_thread": False})
else:
    logger.info(f"🚀 Database: PostgreSQL at {DATABASE_URL}")
    engine = create_engine(DATABASE_URL, pool_size=20, max_overflow=30, pool_pre_ping=True)

SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)
Base = declarative_base()

def get_db() -> Generator[Session, None, None]:
    """FastAPI dependency for database sessions."""
    db = SessionLocal()
    try:
        if not USE_SQLITE_DEV:
            db.execute(text("CREATE EXTENSION IF NOT EXISTS vector"))
            db.commit()
        yield db
    finally:
        db.close()

# Models
class Taxonomy(Base):
    """Root taxonomy (e.g., 'Categories')"""
    __tablename__ = "taxonomies"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), primary_key=True, default=uuid.uuid4)
    name = Column(String(255), nullable=False, unique=True, index=True)
    presentation = Column(String(255))
    position = Column(Integer, default=0)
    public_metadata = Column(JSONB if not USE_SQLITE_DEV else Text)
    private_metadata = Column(JSONB if not USE_SQLITE_DEV else Text)
    created_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC))
    updated_at = Column(
        DateTime(timezone=True),
        default=lambda: datetime.now(UTC),
    onupdate=lambda: datetime.now(UTC)
)

    taxons = relationship("Taxon", back_populates="taxonomy")

class Taxon(Base):
    """Hierarchical category (e.g., Apparel > Topwear > Shirts)"""
    __tablename__ = "taxa"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), primary_key=True, default=uuid.uuid4)
    taxonomy_id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), ForeignKey("eshopdb.taxonomies.id"), nullable=False, index=True)
    parent_id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), ForeignKey("eshopdb.taxa.id"), nullable=True, index=True)
    name = Column(String(255), nullable=False, index=True)
    presentation = Column(String(255))
    description = Column(Text)
    permalink = Column(String(750), nullable=False, index=True)
    pretty_name = Column(String(500))
    position = Column(Integer, default=0)
    lft = Column(Integer, default=0, index=True)
    rgt = Column(Integer, default=0, index=True)
    depth = Column(Integer, default=0)
    hide_from_nav = Column(Boolean, default=False)
    automatic = Column(Boolean, default=False)
    rules_match_policy = Column(String(50), default="all")
    sort_order = Column(String(50), default="manual")
    marked_for_regenerate_taxon_products = Column(Boolean, default=False)
    public_metadata = Column(JSONB if not USE_SQLITE_DEV else Text)
    private_metadata = Column(JSONB if not USE_SQLITE_DEV else Text)
    created_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC))
    updated_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC), onupdate=lambda: datetime.now(UTC))
    taxonomy = relationship("Taxonomy", back_populates="taxons")
    parent = relationship("Taxon", remote_side=[id], backref="children")
    classifications = relationship("ProductClassification", back_populates="taxon")

class ProductClassification(Base):
    """Product-to-category mapping"""
    __tablename__ = "classification"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), primary_key=True, default=uuid.uuid4)
    product_id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), ForeignKey("eshopdb.products.id"), nullable=False, index=True)
    taxon_id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), ForeignKey("eshopdb.taxa.id"), nullable=False, index=True)
    position = Column(Integer, default=0)
    created_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC))
    updated_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC), onupdate=lambda: datetime.now(UTC))
    product = relationship("Product", back_populates="classifications")
    taxon = relationship("Taxon", back_populates="classifications")

class OptionType(Base):
    """Variant option type (e.g., 'Color', 'Size')"""
    __tablename__ = "option_types"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), primary_key=True, default=uuid.uuid4)
    name = Column(String(100), nullable=False, unique=True, index=True)
    presentation = Column(String(100), nullable=False)
    position = Column(Integer, default=0)
    filterable = Column(Boolean, default=False)
    created_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC))
    updated_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC), onupdate=lambda: datetime.now(UTC))
    option_values = relationship("OptionValue", back_populates="option_type")
    product_option_types = relationship("ProductOptionType", back_populates="option_type")

class OptionValue(Base):
    """Option value (e.g., 'Red', 'Large')"""
    __tablename__ = "option_values"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), primary_key=True, default=uuid.uuid4)
    option_type_id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), ForeignKey("eshopdb.option_types.id"), nullable=False, index=True)
    name = Column(String(255), nullable=False)
    presentation = Column(String(255), nullable=False)
    position = Column(Integer, default=0)
    created_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC))
    updated_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC), onupdate=lambda: datetime.now(UTC))
    option_type = relationship("OptionType", back_populates="option_values")
    variant_option_values = relationship("VariantOptionValue", back_populates="option_value")

class ProductOptionType(Base):
    """Product-to-option-type link"""
    __tablename__ = "product_option_types"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), primary_key=True, default=uuid.uuid4)
    product_id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), ForeignKey("eshopdb.products.id"), nullable=False, index=True)
    option_type_id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), ForeignKey("eshopdb.option_types.id"), nullable=False, index=True)
    position = Column(Integer, default=0)
    created_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC))
    updated_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC), onupdate=lambda: datetime.now(UTC))
    product = relationship("Product", back_populates="product_option_types")
    option_type = relationship("OptionType", back_populates="product_option_types")

class VariantOptionValue(Base):
    """Variant-to-option-value link (e.g., SKU-001 has Color=Red)"""
    __tablename__ = "variant_option_values"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), primary_key=True, default=uuid.uuid4)
    variant_id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), ForeignKey("eshopdb.variants.id"), nullable=False, index=True)
    option_value_id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), ForeignKey("eshopdb.option_values.id"), nullable=False, index=True)
    created_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC))
    updated_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC), onupdate=lambda: datetime.now(UTC))
    variant = relationship("ProductVariant", back_populates="variant_option_values")
    option_value = relationship("OptionValue", back_populates="variant_option_values")

class PropertyType(Base):
    """Product property type (e.g., 'Material', 'Brand')"""
    __tablename__ = "property_types"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), primary_key=True, default=uuid.uuid4)
    name = Column(String(255), nullable=False, unique=True, index=True)
    presentation = Column(String(255))
    created_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC))
    updated_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC), onupdate=lambda: datetime.now(UTC))
    product_property_types = relationship("ProductPropertyType", back_populates="property_type")

class ProductPropertyType(Base):
    """Product property value (e.g., Material='Cotton')"""
    __tablename__ = "product_property_types"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), primary_key=True, default=uuid.uuid4)
    product_id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), ForeignKey("eshopdb.products.id"), nullable=False, index=True)
    property_type_id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), ForeignKey("eshopdb.property_types.id"), nullable=False, index=True)
    property_type_value = Column(String(1000))
    position = Column(Integer, default=0)
    created_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC))
    updated_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC), onupdate=lambda: datetime.now(UTC))
    product = relationship("Product", back_populates="product_properties")
    property_type = relationship("PropertyType", back_populates="product_property_types")

class Product(Base):
    """Core product model with train/val/test split metadata"""
    __tablename__ = "products"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), primary_key=True, default=uuid.uuid4)
    name = Column(String(255), nullable=False)
    presentation = Column(String(255))
    slug = Column(String(255), unique=True, nullable=False, index=True)
    description = Column(Text)
    status = Column(Integer, default=0)
    public_metadata = Column(JSONB if not USE_SQLITE_DEV else Text)  # Contains 'split': train/val/test
    created_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC))
    updated_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC), onupdate=lambda: datetime.now(UTC))
    variants = relationship("ProductVariant", back_populates="product")
    images = relationship("ProductImage", back_populates="product")
    classifications = relationship("ProductClassification", back_populates="product")
    product_option_types = relationship("ProductOptionType", back_populates="product")
    product_properties = relationship("ProductPropertyType", back_populates="product")

class ProductVariant(Base):
    """Product variant (SKU with size/color combination)"""
    __tablename__ = "variants"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), primary_key=True, default=uuid.uuid4)
    product_id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), ForeignKey("eshopdb.products.id"), nullable=False, index=True)
    sku = Column(String(255), index=True)
    is_master = Column(Boolean, default=False)
    created_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC))
    updated_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC), onupdate=lambda: datetime.now(UTC))
    product = relationship("Product", back_populates="variants")
    variant_option_values = relationship("VariantOptionValue", back_populates="variant")
    stock_items = relationship("StockItem", back_populates="variant")
    images = relationship("ProductImage", back_populates="variant")
    prices = relationship("Price", back_populates="variant")

class Price(Base):
    """Variant pricing (multi-currency support)"""
    __tablename__ = "prices"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), primary_key=True, default=uuid.uuid4)
    variant_id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), ForeignKey("eshopdb.variants.id"), nullable=False, index=True)
    amount = Column(DECIMAL(18, 2))
    currency = Column(String(3), nullable=False)
    created_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC))
    updated_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC), onupdate=lambda: datetime.now(UTC))
    variant = relationship("ProductVariant", back_populates="prices")

class ProductImage(Base):
    """
    CRITICAL THESIS MODEL: Images with 4 embedding architectures
    
    Embeddings:
    - embedding_mobilenet (576-dim): MobileNetV3-Small CNN
    - embedding_efficientnet (1280-dim): EfficientNet-B0 CNN
    - embedding_clip (512-dim): CLIP ViT-B/16 Transformer
    - embedding_dino (384-dim): DINO ViT-S/16 Transformer
    
    type='Search' images are used for similarity search
    """
    __tablename__ = "product_images"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), primary_key=True, default=uuid.uuid4)
    product_id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), ForeignKey("eshopdb.products.id"), nullable=True, index=True)
    variant_id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), ForeignKey("eshopdb.variants.id"), nullable=True, index=True)
    url = Column(String(2048), nullable=False)
    alt = Column(String(255))
    type = Column(String(50), index=True)
    position = Column(Integer, default=0)
    content_type = Column(String(50))
    
    # MobileNetV3 embeddings
    embedding_mobilenet = Column(Vector(576) if not USE_SQLITE_DEV else Text)
    embedding_mobilenet_model = Column(String(50))
    embedding_mobilenet_generated_at = Column(DateTime)
    
    # EfficientNet embeddings
    embedding_efficientnet = Column(Vector(1280) if not USE_SQLITE_DEV else Text)
    embedding_efficientnet_model = Column(String(50))
    embedding_efficientnet_generated_at = Column(DateTime)
    
    # ResNet50 embeddings
    embedding_resnet = Column(Vector(2048) if not USE_SQLITE_DEV else Text)
    embedding_resnet_model = Column(String(50))
    embedding_resnet_generated_at = Column(DateTime)

    # ConvNeXt embeddings
    embedding_convnext = Column(Vector(768) if not USE_SQLITE_DEV else Text)
    embedding_convnext_model = Column(String(50))
    embedding_convnext_generated_at = Column(DateTime)

    # CLIP embeddings
    embedding_clip = Column(Vector(512) if not USE_SQLITE_DEV else Text)
    embedding_clip_model = Column(String(50))
    embedding_clip_generated_at = Column(DateTime)
    embedding_clip_checksum = Column(String(64))

    # Fashion-CLIP embeddings
    embedding_fclip = Column(Vector(512) if not USE_SQLITE_DEV else Text)
    embedding_fclip_model = Column(String(50))
    embedding_fclip_generated_at = Column(DateTime)
    
    # DINO embeddings
    embedding_dino = Column(Vector(384) if not USE_SQLITE_DEV else Text)
    embedding_dino_model = Column(String(50))
    embedding_dino_generated_at = Column(DateTime)
    
    created_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC))
    updated_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC), onupdate=lambda: datetime.now(UTC))
    product = relationship("Product", back_populates="images")
    variant = relationship("ProductVariant", back_populates="images")

class StockLocation(Base):
    """Warehouse/stock location"""
    __tablename__ = "stock_locations"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), primary_key=True, default=uuid.uuid4)
    name = Column(String(255), unique=True, nullable=False, index=True)
    presentation = Column(String(255))
    active = Column(Boolean, default=True)
    is_default = Column('default', Boolean, default=False)
    created_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC))
    updated_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC), onupdate=lambda: datetime.now(UTC))

class StockItem(Base):
    """Inventory record"""
    __tablename__ = "stock_items"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), primary_key=True, default=uuid.uuid4)
    variant_id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), ForeignKey("eshopdb.variants.id"), nullable=False, index=True)
    stock_location_id = Column(UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36), ForeignKey("eshopdb.stock_locations.id"), nullable=False, index=True)
    quantity_on_hand = Column(Integer, default=0)
    created_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC))
    updated_at = Column(DateTime(timezone=True), default=lambda: datetime.now(UTC), onupdate=lambda: datetime.now(UTC))
    variant = relationship("ProductVariant", back_populates="stock_items")
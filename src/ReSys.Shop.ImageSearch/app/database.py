"""
Database Module for Fashion Image Search Thesis
================================================

SQLAlchemy ORM models for e-commerce fashion database with multi-model embeddings.
Updated to match the ReSys.Shop schema.

Architecture Support:
- PostgreSQL with pgvector (production)
- SQLite with JSON fallback (development)

Author: [Your Name]
Thesis: Building a Fashion E-commerce Application with Recommendation and Image-based Product Search
Date: December 2025
"""

import uuid
import logging
from datetime import datetime, timezone
from typing import Generator
from sqlalchemy import (
    DECIMAL,
    Boolean,
    Column,
    DateTime,
    ForeignKey,
    Integer,
    BigInteger,
    String,
    Text,
    create_engine,
    text,
)
from sqlalchemy.dialects.postgresql import JSONB, UUID
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import relationship, sessionmaker, Session
from pgvector.sqlalchemy import Vector

from app.config import settings

# Configure logging
logger = logging.getLogger(__name__)

# Database Configuration
DATABASE_URL = (
    settings.DATABASE_URL or "postgresql://postgres:12345678@localhost:5432/eshopdb"
)
USE_SQLITE_DEV = DATABASE_URL.startswith("sqlite")

if USE_SQLITE_DEV:
    logger.info(f"🔧 Database: SQLite at {DATABASE_URL}")
    engine = create_engine(DATABASE_URL, connect_args={"check_same_thread": False})
else:
    logger.info(f"🚀 Database: PostgreSQL at {DATABASE_URL}")
    engine = create_engine(
        DATABASE_URL, pool_size=20, max_overflow=30, pool_pre_ping=True
    )

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
    id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        primary_key=True,
        default=uuid.uuid4,
    )
    name = Column(String(100), nullable=False)
    presentation = Column(String(1000), nullable=False)
    position = Column(Integer, nullable=False, default=1)
    public_metadata = Column(JSONB if not USE_SQLITE_DEV else Text)
    private_metadata = Column(JSONB if not USE_SQLITE_DEV else Text)
    created_at = Column(
        DateTime(timezone=True),
        nullable=False,
        default=lambda: datetime.now(timezone.utc),
    )
    created_by = Column(String(100))
    updated_at = Column(DateTime(timezone=True))
    updated_by = Column(String(256))
    version = Column(BigInteger, nullable=False, default=0)

    taxons = relationship("Taxon", back_populates="taxonomy")


class Taxon(Base):
    """Hierarchical category (e.g., Apparel > Topwear > Shirts)"""

    __tablename__ = "taxa"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        primary_key=True,
        default=uuid.uuid4,
    )
    name = Column(String(100), nullable=False)
    presentation = Column(String(1000), nullable=False)
    description = Column(String(500))
    permalink = Column(String(200), nullable=False)
    pretty_name = Column(String(100), nullable=False)
    hide_from_nav = Column(Boolean, nullable=False, default=False)
    position = Column(Integer, nullable=False, default=1)
    lft = Column(Integer, nullable=False, default=0)
    rgt = Column(Integer, nullable=False, default=0)
    depth = Column(Integer, nullable=False, default=0)
    automatic = Column(Boolean, nullable=False, default=False)
    rules_match_policy = Column(String(50), nullable=False, default="all")
    sort_order = Column(String(50), nullable=False, default="manual")
    marked_for_regenerate_taxon_products = Column(
        Boolean, nullable=False, default=False
    )
    taxonomy_id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        ForeignKey("eshopdb.taxonomies.id"),
        nullable=False,
    )
    parent_id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        ForeignKey("eshopdb.taxa.id"),
        nullable=True,
    )
    public_metadata = Column(JSONB if not USE_SQLITE_DEV else Text)
    private_metadata = Column(JSONB if not USE_SQLITE_DEV else Text)
    created_at = Column(
        DateTime(timezone=True),
        nullable=False,
        default=lambda: datetime.now(timezone.utc),
    )
    created_by = Column(String(100))
    updated_at = Column(DateTime(timezone=True))
    updated_by = Column(String(256))
    version = Column(BigInteger, nullable=False, default=0)

    taxonomy = relationship("Taxonomy", back_populates="taxons")
    parent = relationship("Taxon", remote_side=[id], backref="children")
    classifications = relationship("ProductClassification", back_populates="taxon")


class ProductClassification(Base):
    """Product-to-category mapping"""

    __tablename__ = "classification"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        primary_key=True,
        default=uuid.uuid4,
    )
    position = Column(Integer, nullable=False, default=1)
    product_id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        ForeignKey("eshopdb.products.id"),
        nullable=False,
    )
    taxon_id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        ForeignKey("eshopdb.taxa.id"),
        nullable=False,
    )
    created_at = Column(
        DateTime(timezone=True),
        nullable=False,
        default=lambda: datetime.now(timezone.utc),
    )
    created_by = Column(String(100))
    updated_at = Column(DateTime(timezone=True))
    updated_by = Column(String(256))

    product = relationship("Product", back_populates="classifications")
    taxon = relationship("Taxon", back_populates="classifications")


class OptionType(Base):
    """Variant option type (e.g., 'Color', 'Size')"""

    __tablename__ = "option_types"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        primary_key=True,
        default=uuid.uuid4,
    )
    name = Column(String(255), nullable=False, unique=True, index=True)
    presentation = Column(String(1000), nullable=False)
    position = Column(Integer, nullable=False, default=1)
    filterable = Column(Boolean, nullable=False, default=False)
    created_at = Column(
        DateTime(timezone=True),
        nullable=False,
        default=lambda: datetime.now(timezone.utc),
    )
    created_by = Column(String(100))
    updated_at = Column(DateTime(timezone=True))
    updated_by = Column(String(256))
    option_values = relationship("OptionValue", back_populates="option_type")
    product_option_types = relationship(
        "ProductOptionType", back_populates="option_type"
    )


class OptionValue(Base):
    """Option value (e.g., 'Red', 'Large')"""

    __tablename__ = "option_values"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        primary_key=True,
        default=uuid.uuid4,
    )
    option_type_id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        ForeignKey("eshopdb.option_types.id"),
        nullable=False,
    )
    name = Column(String(255), nullable=False)
    presentation = Column(String(1000), nullable=False)
    position = Column(Integer, nullable=False, default=1)
    created_at = Column(
        DateTime(timezone=True),
        nullable=False,
        default=lambda: datetime.now(timezone.utc),
    )
    created_by = Column(String(100))
    updated_at = Column(DateTime(timezone=True))
    updated_by = Column(String(256))
    version = Column(BigInteger, nullable=False, default=0)

    option_type = relationship("OptionType", back_populates="option_values")
    variant_option_values = relationship(
        "VariantOptionValue", back_populates="option_value"
    )


class ProductOptionType(Base):
    """Product-to-option-type link"""

    __tablename__ = "product_option_types"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        primary_key=True,
        default=uuid.uuid4,
    )
    product_id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        ForeignKey("eshopdb.products.id"),
        nullable=False,
    )
    option_type_id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        ForeignKey("eshopdb.option_types.id"),
        nullable=False,
    )
    position = Column(Integer, nullable=False, default=1)
    created_at = Column(
        DateTime(timezone=True),
        nullable=False,
        default=lambda: datetime.now(timezone.utc),
    )
    created_by = Column(String(100))
    updated_at = Column(DateTime(timezone=True))
    updated_by = Column(String(256))

    product = relationship("Product", back_populates="product_option_types")
    option_type = relationship("OptionType", back_populates="product_option_types")


class VariantOptionValue(Base):
    """Variant-to-option-value link"""

    __tablename__ = "variant_option_values"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        primary_key=True,
        default=uuid.uuid4,
    )
    variant_id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        ForeignKey("eshopdb.variants.id"),
        nullable=False,
    )
    option_value_id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        ForeignKey("eshopdb.option_values.id"),
        nullable=False,
    )
    created_at = Column(
        DateTime(timezone=True),
        nullable=False,
        default=lambda: datetime.now(timezone.utc),
    )
    created_by = Column(String(100))
    updated_at = Column(DateTime(timezone=True))
    updated_by = Column(String(256))

    variant = relationship("ProductVariant", back_populates="variant_option_values")
    option_value = relationship("OptionValue", back_populates="variant_option_values")


class PropertyType(Base):
    """Product property type (e.g., 'Material', 'Brand')"""

    __tablename__ = "property_types"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        primary_key=True,
        default=uuid.uuid4,
    )
    name = Column(String(100), nullable=False, unique=True, index=True)
    presentation = Column(String(1000), nullable=False)
    kind = Column(String(50), nullable=False, default="String")
    filterable = Column(Boolean, nullable=False, default=False)
    display_on = Column(String(50), nullable=False, default="ProductDetails")
    position = Column(Integer, nullable=False, default=1)
    created_at = Column(
        DateTime(timezone=True),
        nullable=False,
        default=lambda: datetime.now(timezone.utc),
    )
    created_by = Column(String(100))
    updated_at = Column(DateTime(timezone=True))
    updated_by = Column(String(256))
    version = Column(BigInteger, nullable=False, default=0)

    product_property_types = relationship(
        "ProductPropertyType", back_populates="property_type"
    )


class ProductPropertyType(Base):
    """Product property value (e.g., Material='Cotton')"""

    __tablename__ = "product_property_types"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        primary_key=True,
        default=uuid.uuid4,
    )
    product_id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        ForeignKey("eshopdb.products.id"),
        nullable=False,
    )
    property_type_id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        ForeignKey("eshopdb.property_types.id"),
        nullable=False,
    )
    property_type_value = Column(String(5000), nullable=False)
    position = Column(Integer, nullable=False, default=1)
    created_at = Column(
        DateTime(timezone=True),
        nullable=False,
        default=lambda: datetime.now(timezone.utc),
    )
    created_by = Column(String(100))
    updated_at = Column(DateTime(timezone=True))
    updated_by = Column(String(256))

    product = relationship("Product", back_populates="product_properties")
    property_type = relationship(
        "PropertyType", back_populates="product_property_types"
    )


class Product(Base):
    """Core product model"""

    __tablename__ = "products"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        primary_key=True,
        default=uuid.uuid4,
    )
    name = Column(String(100), nullable=False)
    presentation = Column(Text, nullable=False)
    description = Column(String(5000))
    slug = Column(String(200), nullable=False, unique=True)
    status = Column(String(100), nullable=False, default="Active")
    is_digital = Column(Boolean, nullable=False, default=False)
    marked_for_regenerate_taxon_products = Column(
        Boolean, nullable=False, default=False
    )
    is_deleted = Column(Boolean, nullable=False, default=False)
    public_metadata = Column(JSONB if not USE_SQLITE_DEV else Text)
    private_metadata = Column(JSONB if not USE_SQLITE_DEV else Text)
    created_at = Column(
        DateTime(timezone=True),
        nullable=False,
        default=lambda: datetime.now(timezone.utc),
    )
    created_by = Column(String(100))
    updated_at = Column(DateTime(timezone=True))
    updated_by = Column(String(256))
    version = Column(BigInteger, nullable=False, default=0)

    variants = relationship("ProductVariant", back_populates="product")
    images = relationship("ProductImage", back_populates="product")
    classifications = relationship("ProductClassification", back_populates="product")
    product_option_types = relationship("ProductOptionType", back_populates="product")
    product_properties = relationship("ProductPropertyType", back_populates="product")


class ProductVariant(Base):
    """Product variant (SKU with size/color combination)"""

    __tablename__ = "variants"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        primary_key=True,
        default=uuid.uuid4,
    )
    product_id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        ForeignKey("eshopdb.products.id"),
        nullable=False,
    )
    is_master = Column(Boolean, nullable=False, default=False)
    sku = Column(String(255))
    track_inventory = Column(Boolean, nullable=False, default=True)
    position = Column(Integer, nullable=False, default=1)
    is_deleted = Column(Boolean, nullable=False, default=False)
    public_metadata = Column(JSONB if not USE_SQLITE_DEV else Text)
    private_metadata = Column(JSONB if not USE_SQLITE_DEV else Text)
    created_at = Column(
        DateTime(timezone=True),
        nullable=False,
        default=lambda: datetime.now(timezone.utc),
    )
    created_by = Column(String(100))
    updated_at = Column(DateTime(timezone=True))
    updated_by = Column(String(256))
    version = Column(BigInteger, nullable=False, default=0)

    product = relationship("Product", back_populates="variants")
    images = relationship("ProductImage", back_populates="variant")
    stock_items = relationship("StockItem", back_populates="variant")
    prices = relationship("Price", back_populates="variant", cascade="all, delete-orphan")
    variant_option_values = relationship(
        "VariantOptionValue", back_populates="variant"
    )
    cost_price = Column(DECIMAL(18, 4))
    cost_currency = Column(String(3), default="USD")


class Price(Base):
    """Price record for a variant"""

    __tablename__ = "prices"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        primary_key=True,
        default=uuid.uuid4,
    )
    variant_id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        ForeignKey("eshopdb.variants.id"),
        nullable=False,
    )
    amount = Column(DECIMAL(18, 4), nullable=False)
    currency = Column(String(3), nullable=False, default="USD")
    compare_at_amount = Column(DECIMAL(18, 4))
    created_at = Column(
        DateTime(timezone=True),
        nullable=False,
        default=lambda: datetime.now(timezone.utc),
    )
    updated_at = Column(DateTime(timezone=True))
    version = Column(BigInteger, nullable=False, default=0)

    variant = relationship("ProductVariant", back_populates="prices")


class ProductImage(Base):
    """Images with multi-model embeddings"""

    __tablename__ = "product_images"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        primary_key=True,
        default=uuid.uuid4,
    )
    product_id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        ForeignKey("eshopdb.products.id"),
        nullable=True,
    )
    variant_id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        ForeignKey("eshopdb.variants.id"),
        nullable=True,
    )
    url = Column(String(2048), nullable=False)
    alt = Column(String(255))
    type = Column(String(50), nullable=False)
    position = Column(Integer, nullable=False, default=1)
    content_type = Column(String(50), nullable=False)

    # Embeddings
    embedding_efficientnet = Column(Vector(1280) if not USE_SQLITE_DEV else Text)
    embedding_efficientnet_model = Column(String(50))
    embedding_efficientnet_generated_at = Column(DateTime(timezone=True))
    embedding_efficientnet_checksum = Column(String(64))

    embedding_convnext = Column(Vector(768) if not USE_SQLITE_DEV else Text)
    embedding_convnext_model = Column(String(50))
    embedding_convnext_generated_at = Column(DateTime(timezone=True))
    embedding_convnext_checksum = Column(String(64))

    embedding_clip = Column(Vector(512) if not USE_SQLITE_DEV else Text)
    embedding_clip_model = Column(String(50))
    embedding_clip_generated_at = Column(DateTime(timezone=True))
    embedding_clip_checksum = Column(String(64))

    embedding_fclip = Column(Vector(512) if not USE_SQLITE_DEV else Text)
    embedding_fclip_model = Column(String(50))
    embedding_fclip_generated_at = Column(DateTime(timezone=True))
    embedding_fclip_checksum = Column(String(64))

    embedding_dino = Column(Vector(384) if not USE_SQLITE_DEV else Text)
    embedding_dino_model = Column(String(50))
    embedding_dino_generated_at = Column(DateTime(timezone=True))
    embedding_dino_checksum = Column(String(64))

    created_at = Column(
        DateTime(timezone=True),
        nullable=False,
        default=lambda: datetime.now(timezone.utc),
    )
    created_by = Column(String(100))
    updated_at = Column(DateTime(timezone=True))
    updated_by = Column(String(256))

    product = relationship("Product", back_populates="images")
    variant = relationship("ProductVariant", back_populates="images")


class StockLocation(Base):
    """Warehouse/stock location"""

    __tablename__ = "stock_locations"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        primary_key=True,
        default=uuid.uuid4,
    )
    name = Column(String(100), nullable=False)
    presentation = Column(String(1000), nullable=False)
    type = Column(Integer, nullable=False)  # 1 = Warehouse
    active = Column(Boolean, nullable=False, default=True)
    is_default = Column("default", Boolean, nullable=False, default=False)
    is_deleted = Column(Boolean, nullable=False, default=False)
    created_at = Column(
        DateTime(timezone=True),
        nullable=False,
        default=lambda: datetime.now(timezone.utc),
    )
    created_by = Column(String(100))
    updated_at = Column(DateTime(timezone=True))
    updated_by = Column(String(256))
    version = Column(BigInteger, nullable=False, default=0)

    stock_items = relationship("StockItem", back_populates="stock_location")


class StockItem(Base):
    """Inventory record"""

    __tablename__ = "stock_items"
    __table_args__ = {"schema": "eshopdb"}
    id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        primary_key=True,
        default=uuid.uuid4,
    )
    variant_id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        ForeignKey("eshopdb.variants.id"),
        nullable=False,
    )
    stock_location_id = Column(
        UUID(as_uuid=True) if not USE_SQLITE_DEV else String(36),
        ForeignKey("eshopdb.stock_locations.id"),
        nullable=False,
    )
    sku = Column(Text, nullable=False)
    quantity_on_hand = Column(Integer, nullable=False, default=0)
    backorderable = Column(Boolean, nullable=False, default=True)
    max_backorder_quantity = Column(Integer, nullable=False, default=0)
    created_at = Column(
        DateTime(timezone=True),
        nullable=False,
        default=lambda: datetime.now(timezone.utc),
    )
    created_by = Column(String(100))
    updated_at = Column(DateTime(timezone=True))
    updated_by = Column(String(256))
    version = Column(BigInteger, nullable=False, default=0)

    variant = relationship("ProductVariant", back_populates="stock_items")
    stock_location = relationship("StockLocation", back_populates="stock_items")

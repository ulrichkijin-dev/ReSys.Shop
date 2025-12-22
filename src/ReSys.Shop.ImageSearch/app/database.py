"""
This module defines the SQLAlchemy ORM models that map to the PostgreSQL database schema.

The schema is designed to mirror the core domain entities of the ReSys.Shop e-commerce
platform, particularly those relevant to the product catalog. It uses pgvector for
storing and querying image embeddings.

Key features of the schema include:
- A hierarchical product categorization system using Taxonomies and Taxons.
- A flexible product architecture with Products, Variants, Options, and Properties.
- Support for multiple image embeddings per product image for model comparison.
- Stock and pricing information at the variant level.
"""

import os
import uuid
from datetime import datetime
from dotenv import load_dotenv
import logging

load_dotenv()
logger = logging.getLogger(__name__)

from pgvector.sqlalchemy import Vector
from sqlalchemy import (
    DECIMAL, Boolean, Column, DateTime, ForeignKey,
    Integer, String, Text, create_engine, Index, LargeBinary, text
)
from sqlalchemy.dialects.postgresql import JSONB, UUID
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import relationship, sessionmaker

# --- Database Connection Setup ---
DATABASE_URL = os.getenv(
    "DATABASE_URL", 
    "postgresql://postgres:12345678@localhost:5432/eshopdb"
)

logger.info(f"Connecting to database...")

# Create engine with connection pooling
engine = create_engine(
    DATABASE_URL,
    pool_size=10,
    max_overflow=20,
    pool_pre_ping=True,  # Verify connections before using them
    echo=False  # Set to True for SQL debugging
)

SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)
Base = declarative_base()


def get_db():
    """
    FastAPI dependency to get a DB session for a single request.
    Ensures pgvector extension is enabled on first connection.
    """
    db = SessionLocal()
    try:
        # Ensure the vector extension is enabled (idempotent operation)
        db.execute(text("CREATE EXTENSION IF NOT EXISTS vector"))
        db.commit()
        yield db
    except Exception as e:
        logger.error(f"Database error: {e}")
        db.rollback()
        raise
    finally:
        db.close()


# --- Schema Mappings (SQLAlchemy ORM Models) ---

class Taxonomy(Base):
    """Represents a container for a hierarchy of taxons (e.g., 'Categories', 'Brands')."""
    __tablename__ = "taxonomies"
    
    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    name = Column(String(255), nullable=False, unique=True)
    presentation = Column(String(255))
    position = Column(Integer, default=0)
    public_metadata = Column(JSONB)
    private_metadata = Column(JSONB)
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)

    taxons = relationship("Taxon", back_populates="taxonomy")


class Taxon(Base):
    """Represents a single node (category) within a Taxonomy's hierarchy."""
    __tablename__ = "taxa"
    
    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    taxonomy_id = Column(UUID(as_uuid=True), ForeignKey("taxonomies.id"), nullable=False)
    parent_id = Column(UUID(as_uuid=True), ForeignKey("taxa.id"), nullable=True)
    name = Column(String(255), nullable=False)
    presentation = Column(String(255))
    description = Column(Text)
    permalink = Column(String(750), nullable=False, index=True)
    pretty_name = Column(String(500))
    position = Column(Integer, default=0)
    
    # Nested set model fields for hierarchy traversal
    lft = Column(Integer, default=0, index=True)
    rgt = Column(Integer, default=0, index=True)
    depth = Column(Integer, default=0)
    hide_from_nav = Column(Boolean, default=False)

    # Automatic categorization fields
    automatic = Column(Boolean, default=False)
    rules_match_policy = Column(String(50), default="all")  # 'all' or 'any'
    sort_order = Column(String(50), default="manual")
    marked_for_regenerate_taxon_products = Column(Boolean, default=False)

    # SEO fields
    meta_title = Column(String(255))
    meta_description = Column(String(500))
    meta_keywords = Column(String(255))

    public_metadata = Column(JSONB)
    private_metadata = Column(JSONB)
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)

    taxonomy = relationship("Taxonomy", back_populates="taxons")
    parent = relationship("Taxon", remote_side=[id], backref="children")
    classifications = relationship("ProductClassification", back_populates="taxon")
    taxon_images = relationship("TaxonImage", back_populates="taxon")
    taxon_rules = relationship("TaxonRule", back_populates="taxon")


class TaxonImage(Base):
    """An image associated with a Taxon (e.g., category icon or banner)."""
    __tablename__ = "taxon_images"
    
    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    taxon_id = Column(UUID(as_uuid=True), ForeignKey("taxa.id"), nullable=False)
    url = Column(String(2048), nullable=False)
    alt = Column(String(255))
    type = Column(String(50))  # e.g., 'Icon', 'Banner'
    position = Column(Integer, default=0)
    public_metadata = Column(JSONB)
    private_metadata = Column(JSONB)
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)

    taxon = relationship("Taxon", back_populates="taxon_images")


class TaxonRule(Base):
    """A rule for automatically classifying products into an automatic Taxon."""
    __tablename__ = "taxon_rules"
    
    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    taxon_id = Column(UUID(as_uuid=True), ForeignKey("taxa.id"), nullable=False)
    type = Column(String(100), nullable=False)
    value = Column(String(500), nullable=False)
    match_policy = Column(String(50), default="is_equal_to")
    property_name = Column(String(100))
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)

    taxon = relationship("Taxon", back_populates="taxon_rules")


class ProductClassification(Base):
    """A join table linking a Product to a Taxon, representing its category."""
    __tablename__ = "classification"
    
    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    product_id = Column(UUID(as_uuid=True), ForeignKey("products.id"), nullable=False)
    taxon_id = Column(UUID(as_uuid=True), ForeignKey("taxa.id"), nullable=False)
    position = Column(Integer, default=0)
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)

    product = relationship("Product", back_populates="classifications")
    taxon = relationship("Taxon", back_populates="classifications")


class OptionType(Base):
    """
    Defines a type of product option (e.g., 'Color', 'Size') that is used to
    create distinct product variants.
    """
    __tablename__ = "option_types"
    
    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    name = Column(String(100), nullable=False, unique=True)
    presentation = Column(String(100), nullable=False)
    position = Column(Integer, default=0)
    filterable = Column(Boolean, default=False)
    public_metadata = Column(JSONB)
    private_metadata = Column(JSONB)
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)

    option_values = relationship("OptionValue", back_populates="option_type")
    product_option_types = relationship("ProductOptionType", back_populates="option_type")


class OptionValue(Base):
    """A specific value for an OptionType (e.g., 'Red' for 'Color', 'S' for 'Size')."""
    __tablename__ = "option_values"
    
    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    option_type_id = Column(UUID(as_uuid=True), ForeignKey("option_types.id"), nullable=False)
    name = Column(String(255), nullable=False)
    presentation = Column(String(255), nullable=False)
    position = Column(Integer, default=0)
    public_metadata = Column(JSONB)
    private_metadata = Column(JSONB)
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)

    option_type = relationship("OptionType", back_populates="option_values")
    variant_option_values = relationship("VariantOptionValue", back_populates="option_value")


class ProductOptionType(Base):
    """A join table linking a Product to an OptionType it uses."""
    __tablename__ = "product_option_types"
    
    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    product_id = Column(UUID(as_uuid=True), ForeignKey("products.id"), nullable=False)
    option_type_id = Column(UUID(as_uuid=True), ForeignKey("option_types.id"), nullable=False)
    position = Column(Integer, default=0)
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)

    product = relationship("Product", back_populates="product_option_types")
    option_type = relationship("OptionType", back_populates="product_option_types")


class VariantOptionValue(Base):
    """A join table linking a ProductVariant to an OptionValue that defines it."""
    __tablename__ = "variant_option_values"
    
    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    variant_id = Column(UUID(as_uuid=True), ForeignKey("variants.id"), nullable=False)
    option_value_id = Column(UUID(as_uuid=True), ForeignKey("option_values.id"), nullable=False)
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)

    variant = relationship("ProductVariant", back_populates="variant_option_values")
    option_value = relationship("OptionValue", back_populates="variant_option_values")


class PropertyType(Base):
    """A definable attribute for a product (e.g., 'Material', 'Brand')."""
    __tablename__ = "property_types"
    
    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    name = Column(String(255), nullable=False, unique=True)
    presentation = Column(String(255))
    filter_param = Column(String(255), index=True)
    kind = Column(String(50), default='ShortText')
    filterable = Column(Boolean, default=False)
    display_on = Column(String(50), default='Both')
    position = Column(Integer, default=0)
    public_metadata = Column(JSONB)
    private_metadata = Column(JSONB)
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)

    product_property_types = relationship("ProductPropertyType", back_populates="property_type")


class ProductPropertyType(Base):
    """A join table storing the specific value of a PropertyType for a Product."""
    __tablename__ = "product_property_types"
    
    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    product_id = Column(UUID(as_uuid=True), ForeignKey("products.id"), nullable=False)
    property_type_id = Column(UUID(as_uuid=True), ForeignKey("property_types.id"), nullable=False)
    property_type_value = Column(String(1000))
    position = Column(Integer, default=0)
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)

    product = relationship("Product", back_populates="product_properties")
    property_type = relationship("PropertyType", back_populates="product_property_types")


class Product(Base):
    """The aggregate root for a product, containing all its variants, images, and metadata."""
    __tablename__ = "products"

    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    name = Column(String(255), nullable=False)
    presentation = Column(String(255))
    slug = Column(String(255), unique=True, nullable=False, index=True)
    description = Column(Text)
    status = Column(Integer, default=0)  # 0: Draft, 1: Active, 2: Archived
    is_digital = Column(Boolean, default=False)
    available_on = Column(DateTime)
    make_active_at = Column(DateTime)
    discontinue_on = Column(DateTime)

    meta_title = Column(String(255))
    meta_description = Column(String(500))
    meta_keywords = Column(String(255))

    public_metadata = Column(JSONB)
    private_metadata = Column(JSONB)
    marked_for_regenerate_taxon_products = Column(Boolean, default=False)

    is_deleted = Column(Boolean, default=False)
    deleted_at = Column(DateTime)
    deleted_by = Column(String(255))

    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)

    variants = relationship("ProductVariant", back_populates="product")
    images = relationship("ProductImage", back_populates="product")
    classifications = relationship("ProductClassification", back_populates="product")
    product_option_types = relationship("ProductOptionType", back_populates="product")
    product_properties = relationship("ProductPropertyType", back_populates="product")
    reviews = relationship("Review", back_populates="product")


class ProductVariant(Base):
    """A specific, sellable version of a Product (e.g., 'Red T-Shirt, Size M')."""
    __tablename__ = "variants"

    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    product_id = Column(UUID(as_uuid=True), ForeignKey("products.id"), nullable=False)
    sku = Column(String(255), index=True)
    barcode = Column(String(100))
    is_master = Column(Boolean, default=False)
    position = Column(Integer, default=0)

    weight = Column(DECIMAL(18, 4))
    height = Column(DECIMAL(18, 4))
    width = Column(DECIMAL(18, 4))
    depth = Column(DECIMAL(18, 4))
    dimensions_unit = Column(String(10))
    weight_unit = Column(String(10))

    track_inventory = Column(Boolean, default=True)
    cost_price = Column(DECIMAL(18, 2))
    cost_currency = Column(String(3))
    discontinue_on = Column(DateTime)

    public_metadata = Column(JSONB)
    private_metadata = Column(JSONB)

    is_deleted = Column(Boolean, default=False)
    deleted_at = Column(DateTime)
    deleted_by = Column(String(255))
    
    row_version = Column(LargeBinary)

    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)

    product = relationship("Product", back_populates="variants")
    images = relationship("ProductImage", back_populates="variant")
    prices = relationship("Price", back_populates="variant")
    stock_items = relationship("StockItem", back_populates="variant")
    variant_option_values = relationship("VariantOptionValue", back_populates="variant")


class Price(Base):
    """Pricing information for a ProductVariant in a specific currency."""
    __tablename__ = "prices"
    
    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    variant_id = Column(UUID(as_uuid=True), ForeignKey("variants.id"), nullable=False)
    amount = Column(DECIMAL(18, 2))
    currency = Column(String(3), nullable=False)
    compare_at_amount = Column(DECIMAL(18, 2))
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)

    variant = relationship("ProductVariant", back_populates="prices")


class ProductImage(Base):
    """
    An image associated with a Product or a specific ProductVariant.
    Supports multiple embedding types for model comparison.
    """
    __tablename__ = "product_images"

    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    product_id = Column(UUID(as_uuid=True), ForeignKey("products.id"), nullable=True)
    variant_id = Column(UUID(as_uuid=True), ForeignKey("variants.id"), nullable=True)
    url = Column(String(2048), nullable=False)
    alt = Column(String(255))
    type = Column(String(50))  # 'Default', 'Search' etc.
    position = Column(Integer, default=0)
    content_type = Column(String(50))
    width = Column(Integer)
    height = Column(Integer)
    dimensions_unit = Column(String(10))
    public_metadata = Column(JSONB)
    private_metadata = Column(JSONB)

    # Thesis Comparison Embeddings - MobileNetV3
    embedding_mobilenet = Column(Vector(576))
    embedding_mobilenet_model = Column(String(50))
    embedding_mobilenet_generated_at = Column(DateTime)
    embedding_mobilenet_checksum = Column(String(64))

    # Thesis Comparison Embeddings - EfficientNet
    embedding_efficientnet = Column(Vector(1280))
    embedding_efficientnet_model = Column(String(50))
    embedding_efficientnet_generated_at = Column(DateTime)
    embedding_efficientnet_checksum = Column(String(64))

    # Thesis Comparison Embeddings - CLIP
    embedding_clip = Column(Vector(768))
    embedding_clip_model = Column(String(50))
    embedding_clip_generated_at = Column(DateTime)
    embedding_clip_checksum = Column(String(64))

    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)

    product = relationship("Product", back_populates="images")
    variant = relationship("ProductVariant", back_populates="images")


class Review(Base):
    """A user-submitted review for a Product."""
    __tablename__ = "reviews"
    
    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    product_id = Column(UUID(as_uuid=True), ForeignKey("products.id"), nullable=False)
    user_id = Column(String(255), nullable=False)
    rating = Column(Integer, nullable=False)
    title = Column(String(100))
    comment = Column(Text)
    status = Column(Integer, default=0)  # 0: Pending, 1: Approved, 2: Rejected
    
    moderated_by = Column(String(255))
    moderated_at = Column(DateTime)
    moderation_notes = Column(Text)
    
    helpful_count = Column(Integer, default=0)
    not_helpful_count = Column(Integer, default=0)
    is_verified_purchase = Column(Boolean, default=False)
    order_id = Column(UUID(as_uuid=True))
    
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)
    
    product = relationship("Product", back_populates="reviews")


class StockLocation(Base):
    """A physical or virtual location where stock is held (e.g., a warehouse)."""
    __tablename__ = "stock_locations"

    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    name = Column(String(255), unique=True, nullable=False)
    presentation = Column(String(255))
    active = Column(Boolean, default=True)
    is_default = Column('default', Boolean, default=False)
    type = Column(Integer, default=1)  # 1 = Warehouse
    ship_enabled = Column(Boolean, default=True)
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)


class StockItem(Base):
    """Represents the quantity of a ProductVariant at a specific StockLocation."""
    __tablename__ = "stock_items"

    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    variant_id = Column(UUID(as_uuid=True), ForeignKey("variants.id"), nullable=False)
    stock_location_id = Column(UUID(as_uuid=True), ForeignKey("stock_locations.id"), nullable=False)
    quantity_on_hand = Column(Integer, default=0)
    quantity_reserved = Column(Integer, default=0)
    backorderable = Column(Boolean, default=True)
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)

    variant = relationship("ProductVariant", back_populates="stock_items")
    stock_location = relationship("StockLocation")
"""
Centralized model name mapping for consistency across the application.
Maps model identifiers to database column prefixes and vice versa.

This file ensures consistency between:
- Python API code
- C# domain model (ProductImage entity)
- Database schema
- Configuration files
"""

# Model name to database column prefix mapping (matching C# domain)
MODEL_TO_PREFIX = {
    "efficientnet_b0": "efficientnet",
    "convnext_tiny": "convnext",
    "clip_vit_b16": "clip",
    "fashion_clip": "fclip",
    "dinov2_vits14": "dino",
    # Aliases for backward compatibility
    "efficientnet": "efficientnet",
    "convnext": "convnext",
    "clip": "clip",
    "fclip": "fclip",
    "dino": "dino",
}

# Database column prefix to canonical model name
PREFIX_TO_MODEL = {
    "efficientnet": "efficientnet_b0",
    "convnext": "convnext_tiny",
    "clip": "clip_vit_b16",
    "fclip": "fashion_clip",
    "dino": "dinov2_vits14",
}

# Expected embedding dimensions for validation (matching C# domain)
MODEL_DIMENSIONS = {
    "efficientnet_b0": 1280,
    "convnext_tiny": 768,
    "clip_vit_b16": 512,
    "fashion_clip": 512,
    "dinov2_vits14": 384,
}

# Model architecture grouping for thesis analysis
MODEL_ARCHITECTURES = {
    "efficientnet_b0": "CNN",
    "convnext_tiny": "CNN",
    "clip_vit_b16": "Transformer",
    "fashion_clip": "Transformer",
    "dinov2_vits14": "Transformer",
}


def get_embedding_prefix(model_name: str) -> str:
    """
    Get database column prefix for a model name.

    Args:
        model_name: Canonical model name (e.g., "efficientnet_b0")

    Returns:
        Database column prefix (e.g., "efficientnet")

    Example:
        get_embedding_prefix("efficientnet_b0") -> "efficientnet"
        get_embedding_prefix("fashion_clip") -> "fclip"
    """
    return MODEL_TO_PREFIX.get(model_name, model_name.split("_")[0])


def get_embedding_column(model_name: str) -> str:
    """
    Get full database column name for embeddings.

    Args:
        model_name: Canonical model name (e.g., "efficientnet_b0")

    Returns:
        Full column name (e.g., "embedding_efficientnet")

    Example:
        get_embedding_column("efficientnet_b0") -> "embedding_efficientnet"
        get_embedding_column("fashion_clip") -> "embedding_fclip"
    """
    prefix = get_embedding_prefix(model_name)
    return f"embedding_{prefix}"


def get_model_dimension(model_name: str) -> int:
    """
    Get expected embedding dimension for a model.

    Args:
        model_name: Canonical model name

    Returns:
        Expected dimension (e.g., 1280 for EfficientNet-B0)
    """
    return MODEL_DIMENSIONS.get(model_name, 0)


def get_canonical_model_name(name_or_prefix: str) -> str:
    """
    Convert any model identifier to canonical model name.

    Args:
        name_or_prefix: Model name or prefix

    Returns:
        Canonical model name

    Example:
        get_canonical_model_name("efficientnet") -> "efficientnet_b0"
        get_canonical_model_name("fclip") -> "fashion_clip"
        get_canonical_model_name("efficientnet_b0") -> "efficientnet_b0"
    """
    # If it's already a canonical name
    if name_or_prefix in MODEL_DIMENSIONS:
        return name_or_prefix
    # If it's a prefix, convert to canonical
    return PREFIX_TO_MODEL.get(name_or_prefix, name_or_prefix)


def get_model_architecture(model_name: str) -> str:
    """
    Get architecture type for a model.

    Args:
        model_name: Canonical model name

    Returns:
        Architecture type: "CNN" or "Transformer"
    """
    return MODEL_ARCHITECTURES.get(model_name, "Unknown")


def get_all_canonical_models() -> list:
    """Get list of all canonical model names."""
    return list(MODEL_DIMENSIONS.keys())


def get_all_prefixes() -> list:
    """Get list of all database column prefixes."""
    return list(PREFIX_TO_MODEL.keys())

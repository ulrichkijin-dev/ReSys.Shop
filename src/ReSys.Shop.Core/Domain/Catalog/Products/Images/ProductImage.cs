using Pgvector;
using Pgvector.EntityFrameworkCore;

using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Catalog.Products.Variants;

namespace ReSys.Shop.Core.Domain.Catalog.Products.Images;

/// <summary>
/// Represents an image asset associated with a product or a specific product variant.
/// This entity extends <see cref="BaseImageAsset"/> and incorporates advanced features like visual embeddings
/// for similarity calculations.
/// </summary>
public sealed class ProductImage : BaseImageAsset, IHasIdentity<Guid>
{
    #region Constants & Configuration

    public static class Constraints
    {
        public const int UrlMaxLength = 2048;
        public const int AltMaxLength = 255;
        public const string PathPrefix = "catalog";
        public const string ImageFolder = "images";

        public static readonly string[] ValidTypes =
        {
            nameof(ProductImageType.Default),
            nameof(ProductImageType.Square),
            nameof(ProductImageType.Thumbnail),
            nameof(ProductImageType.Gallery),
            nameof(ProductImageType.Search)
        };

        public static readonly string[] ValidContentTypes =
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp"
        };
        
        public static readonly string[] ValidDimensionUnits = ["mm", "cm", "in", "ft"];

         /// <summary>
        /// Generates a standardized storage path for a product image based on its associated entities and type.
        /// </summary>
        /// <param name="productId">Optional: The ID of the product the image belongs to.</param>
        /// <param name="variantId">Optional: The ID of the variant the image belongs to.</param>
        /// <param name="type">The <see cref="ProductImageType"/> of the image.</param>
        /// <returns>A formatted string representing the suggested storage path for the image file.</returns>
        public static string GetStorageFolder(Guid? productId, Guid? variantId, string type)
        {
            var entityType = "products";
            var id = productId;
            return $"{PathPrefix}/{ImageFolder}/{entityType}/{id}/{type.ToLower()}";
        }

        /// <summary>
        /// Generates a standardized storage path for a product image based on its associated entities and type.
        /// </summary>
        /// <param name="type">The <see cref="ProductImageType"/> of the image.</param>
        /// <param name="contentType">The MIME content type of the image (e.g., "image/jpeg").</param>
        /// <returns>A formatted string representing the suggested storage path for the image file.</returns>
        public static string GetStorageFileName(string type, string contentType)
        {
            var timestamp = DateTimeOffset.UtcNow.ToString(format: "yyyyMMddHHmmss");
            var fileExtension = contentType.Split(separator: '/')[1];
            return $"{timestamp}_{type.ToLower()}.{fileExtension}";
        }
    }

    #endregion

    public enum ProductImageType
    {
        None = 0,
        Default = 1,
        Square = 2,
        Thumbnail = 3,
        Gallery = 4,
        Search = 5
    }

    #region Errors

    public static class Errors
    {
        public static Error Required => Error.Validation(code: "ProductImage.Required", description: "At least one product image is required.");
        public static Error AlreadyExists(Guid productId, Guid? variantId, string type) => Error.Conflict(code: "ProductImage.AlreadyExists", description: $"Product image of type '{type}' already exists.");
        public static Error NotFound(Guid id) => Error.NotFound(code: "ProductImage.NotFound", description: $"Product image with ID '{id}' was not found.");
        public static Error InvalidType => Error.Validation(code: "ProductImage.InvalidType", description: "Invalid image type.");
        public static Error InvalidContentType => Error.Validation(code: "ProductImage.InvalidContentType", description: "Invalid content type.");
        public static Error InvalidDimensionUnit => Error.Validation(code: "ProductImage.InvalidDimensionUnit", description: "Invalid dimension unit.");
        public static Error InvalidUrl => Error.Validation(code: "ProductImage.InvalidUrl", description: "Invalid URL.");
        
        // Improved Embedding Errors
        public static Error InvalidEmbeddingDimension(string modelName, int expected, int actual) => 
            Error.Validation(code: "PRODUCT_IMAGE_EMBEDDING_DIMENSION_MISMATCH", description: $"Model '{modelName}' expects {expected} dimensions but received {actual}");

        public static Error EmbeddingContainsNaN(string modelName) =>
            Error.Validation(code: "PRODUCT_IMAGE_EMBEDDING_CONTAINS_NAN", description: $"Embedding for model '{modelName}' contains NaN values");

        public static Error EmbeddingContainsInfinity(string modelName) =>
            Error.Validation(code: "PRODUCT_IMAGE_EMBEDDING_CONTAINS_INFINITY", description: $"Embedding for model '{modelName}' contains infinite values");

        public static Error EmbeddingNotProperlyNormalized(string modelName, double actualMagnitude) => 
            Error.Validation(code: "PRODUCT_IMAGE_EMBEDDING_NOT_NORMALIZED", description: $"Embedding for model '{modelName}' magnitude is {actualMagnitude:F4}, expected ~1.0");

        public static Error EmbeddingNotGenerated(string modelName) =>
            Error.Validation(code: "PRODUCT_IMAGE_EMBEDDING_NOT_GENERATED", description: $"One or both images missing embedding for model '{modelName}'");

        public static Error UnsupportedEmbeddingModel(string modelName) =>
            Error.Validation(code: "PRODUCT_IMAGE_UNSUPPORTED_EMBEDDING_MODEL", description: $"Model '{modelName}' is not supported. Use: mobilenet_v3, efficientnet_b0");
    }

    #endregion

    #region Core Properties

    public Guid Id { get; set; }
    public string ContentType { get; set; } = "image/jpeg";
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? DimensionsUnit { get; set; }

    // ---------------------------------------------
    // Supported Embedding Models (Thesis Strategy)
    // ---------------------------------------------

    /// <summary>
    /// EfficientNet-B0 (Production Baseline CNN) - 1280-dim.
    /// Focus: Compound Scaling.
    /// </summary>
    public Vector? EmbeddingEfficientnet { get; set; }
    public string? EmbeddingEfficientnetModel { get; set; } = "efficientnet_b0";
    public DateTimeOffset? EmbeddingEfficientnetGeneratedAt { get; set; }
    public string? EmbeddingEfficientnetChecksum { get; set; }

    /// <summary>
    /// ConvNeXt-Tiny (Modern CNN) - 768-dim.
    /// Focus: Modernized CNN Architecture.
    /// </summary>
    public Vector? EmbeddingConvnext { get; set; }
    public string? EmbeddingConvnextModel { get; set; } = "convnext_tiny";
    public DateTimeOffset? EmbeddingConvnextGeneratedAt { get; set; }
    public string? EmbeddingConvnextChecksum { get; set; }

    /// <summary>
    /// CLIP ViT-B/16 (General Transformer) - 512-dim.
    /// Focus: General Semantic Retrieval.
    /// </summary>
    public Vector? EmbeddingClip { get; set; }
    public string? EmbeddingClipModel { get; set; } = "clip_vit_b16";
    public DateTimeOffset? EmbeddingClipGeneratedAt { get; set; }
    public string? EmbeddingClipChecksum { get; set; }

    /// <summary>
    /// Fashion-CLIP (Domain-Specific Transformer) - 512-dim.
    /// Focus: Semantic Retrieval.
    /// </summary>
    public Vector? EmbeddingFclip { get; set; }
    public string? EmbeddingFclipModel { get; set; } = "fashion_clip";
    public DateTimeOffset? EmbeddingFclipGeneratedAt { get; set; }
    public string? EmbeddingFclipChecksum { get; set; }

    /// <summary>
    /// DINOv2 ViT-S/14 (Self-Supervised Transformer) - 384-dim.
    /// Focus: Visual Structure and Silhouette.
    /// </summary>
    public Vector? EmbeddingDino { get; set; }
    public string? EmbeddingDinoModel { get; set; } = "dinov2_vits14";
    public DateTimeOffset? EmbeddingDinoGeneratedAt { get; set; }
    public string? EmbeddingDinoChecksum { get; set; }

    #endregion

    #region Relationships

    public Guid? ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public Product? Product { get; set; }
    public Variant? Variant { get; set; }

    #endregion

    #region Computed Properties

    public new bool IsDefault => Type == nameof(ProductImageType.Default);
    public bool IsSquare => Type == nameof(ProductImageType.Square);
    public bool IsThumbnail => Type == nameof(ProductImageType.Thumbnail);
    public string AspectRatio => Width.HasValue && Height.HasValue && Height > 0 ? $"{Width}:{Height}" : "unknown";

    public bool HasEmbeddingEfficientnet => EmbeddingEfficientnet != null;
    public bool HasEmbeddingConvnext => EmbeddingConvnext != null;
    public bool HasEmbeddingClip => EmbeddingClip != null;
    public bool HasEmbeddingFclip => EmbeddingFclip != null;
    public bool HasEmbeddingDino => EmbeddingDino != null;
    public bool HasAnyEmbedding => HasEmbeddingEfficientnet || HasEmbeddingConvnext || HasEmbeddingClip || HasEmbeddingFclip || HasEmbeddingDino;

    public double? CalculatedAspectRatio =>
        Width.HasValue && Height.HasValue && Height > 0
            ? Math.Round(value: (double)Width.Value / Height.Value, digits: 4)
            : null;

    public bool MatchesExpectedAspectRatio(double tolerance = 0.02)
    {
        var spec = GetSizeSpec(type: Enum.Parse<ProductImageType>(value: Type));
        if (!spec.AspectRatio.HasValue || !CalculatedAspectRatio.HasValue) return true;
        return Math.Abs(value: CalculatedAspectRatio.Value - spec.AspectRatio.Value) <= tolerance;
    }

    #endregion

    #region Constructors
    private ProductImage() { }
    #endregion

    #region Factory

    public static ErrorOr<ProductImage> Create(
        string url,
        Guid? productId = null,
        Guid? variantId = null,
        string? alt = null,
        int position = 0,
        string type = nameof(ProductImageType.Default),
        string contentType = "image/jpeg",
        int? width = null,
        int? height = null,
        string? dimensionsUnit = null,
        IDictionary<string, object?>? publicMetadata = null,
        IDictionary<string, object?>? privateMetadata = null)
    {
        if (string.IsNullOrWhiteSpace(value: url) || url.Length > Constraints.UrlMaxLength) return Errors.InvalidUrl;
        if (!Constraints.ValidContentTypes.Contains(value: contentType)) return Errors.InvalidContentType;
        if (dimensionsUnit is not null && !Constraints.ValidDimensionUnits.Contains(value: dimensionsUnit)) return Errors.InvalidDimensionUnit;
        if (!Constraints.ValidTypes.Contains(value: type.ToString())) return Errors.InvalidType;

        var image = new ProductImage
        {
            Id = Guid.NewGuid(),
            Url = url.Trim(),
            ProductId = productId,
            VariantId = variantId,
            Alt = alt?.Trim(),
            Position = Math.Max(val1: 0, val2: position),
            Type = type,
            ContentType = contentType,
            Width = width,
            Height = height,
            DimensionsUnit = dimensionsUnit,
            PublicMetadata = new Dictionary<string, object?>(dictionary: publicMetadata ?? new Dictionary<string, object?>()),
            PrivateMetadata = new Dictionary<string, object?>(dictionary: privateMetadata ?? new Dictionary<string, object?>()),
            CreatedAt = DateTimeOffset.UtcNow
        };

        return image;
    }

    #endregion

    #region Business Logic

    public ErrorOr<Deleted> Delete() => Result.Deleted;

    public ErrorOr<ProductImage> Update(
        Guid? variantId = null,
        string? url = null,
        string? alt = null,
        int? position = null,
        string? type = null,
        string? contentType = null,
        int? width = null,
        int? height = null,
        string? dimensionsUnit = null,
        IDictionary<string, object?>? publicMetadata = null,
        IDictionary<string, object?>? privateMetadata = null)
    {
        if (contentType is not null && !Constraints.ValidContentTypes.Contains(value: contentType)) return Errors.InvalidContentType;
        if (dimensionsUnit is not null && !Constraints.ValidDimensionUnits.Contains(value: dimensionsUnit)) return Errors.InvalidDimensionUnit;

        bool changed = false;

        if (url is { Length: > 0 } && url != Url)
        {
            Url = url.Trim();
            // Clear all embeddings if image changes
            EmbeddingEfficientnet = null; EmbeddingEfficientnetGeneratedAt = null; EmbeddingEfficientnetChecksum = null;
            EmbeddingConvnext = null; EmbeddingConvnextGeneratedAt = null; EmbeddingConvnextChecksum = null;
            EmbeddingFclip = null; EmbeddingFclipGeneratedAt = null; EmbeddingFclipChecksum = null;
            EmbeddingDino = null; EmbeddingDinoGeneratedAt = null; EmbeddingDinoChecksum = null;
            changed = true;
        }

        if (variantId != null && variantId != VariantId) { VariantId = variantId; changed = true; }
        if (alt != null && alt != Alt) { Alt = alt.Trim(); changed = true; }
        if (position.HasValue && position != Position) { Position = Math.Max(val1: 0, val2: position.Value); changed = true; }
        if (type != null && type != Type) { Type = type; changed = true; }
        if (contentType != null && contentType != ContentType) { ContentType = contentType; changed = true; }
        if (width.HasValue && width != Width) { Width = width > 0 ? width : null; changed = true; }
        if (height.HasValue && height != Height) { Height = height > 0 ? height : null; changed = true; }
        if (dimensionsUnit != null && dimensionsUnit != DimensionsUnit) { DimensionsUnit = dimensionsUnit; changed = true; }

        if (publicMetadata != null && !PublicMetadata.MetadataEquals(dict2: publicMetadata))
        {
            PublicMetadata = new Dictionary<string, object?>(dictionary: publicMetadata);
            changed = true;
        }

        if (privateMetadata != null && !PrivateMetadata.MetadataEquals(dict2: privateMetadata))
        {
            PrivateMetadata = new Dictionary<string, object?>(dictionary: privateMetadata);
            changed = true;
        }

        if (changed) UpdatedAt = DateTimeOffset.UtcNow;

        return this;
    }

    public ErrorOr<ProductImage> SetEmbedding(string modelName, float[] embedding, string? checksum = null)
    {
        var expectedDimResult = TryGetExpectedDimension(modelName);
        if (expectedDimResult.IsError) return expectedDimResult.Errors;
        int expectedDim = expectedDimResult.Value;

        if (embedding.Length != expectedDim) return Errors.InvalidEmbeddingDimension(modelName, expectedDim, embedding.Length);
        if (embedding.Any(float.IsNaN)) return Errors.EmbeddingContainsNaN(modelName);
        if (embedding.Any(float.IsInfinity)) return Errors.EmbeddingContainsInfinity(modelName);

        double magnitude = Math.Sqrt(embedding.Sum(x => (double)x * x));
        if (Math.Abs(magnitude - 1.0) > 0.05) return Errors.EmbeddingNotProperlyNormalized(modelName, magnitude);

        Vector vector = new Vector(v: embedding);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        string defaultModelVersion = modelName.ToLowerInvariant();
        string finalChecksum = checksum ?? ComputeChecksum(embedding);

        switch (modelName.ToLowerInvariant())
        {
            case "efficientnet_b0":
            case "efficientnet":
                EmbeddingEfficientnet = vector;
                EmbeddingEfficientnetModel = defaultModelVersion;
                EmbeddingEfficientnetGeneratedAt = now;
                EmbeddingEfficientnetChecksum = finalChecksum;
                break;

            case "convnext_tiny":
            case "convnext":
                EmbeddingConvnext = vector;
                EmbeddingConvnextModel = defaultModelVersion;
                EmbeddingConvnextGeneratedAt = now;
                EmbeddingConvnextChecksum = finalChecksum;
                break;

            case "clip_vit_b16":
            case "clip":
                EmbeddingClip = vector;
                EmbeddingClipModel = defaultModelVersion;
                EmbeddingClipGeneratedAt = now;
                EmbeddingClipChecksum = finalChecksum;
                break;

            case "fashion_clip":
            case "fclip":
                EmbeddingFclip = vector;
                EmbeddingFclipModel = defaultModelVersion;
                EmbeddingFclipGeneratedAt = now;
                EmbeddingFclipChecksum = finalChecksum;
                break;

            case "dinov2_vits14":
            case "dinov2":
            case "dino":
                EmbeddingDino = vector;
                EmbeddingDinoModel = defaultModelVersion;
                EmbeddingDinoGeneratedAt = now;
                EmbeddingDinoChecksum = finalChecksum;
                break;

            default:
                return Errors.UnsupportedEmbeddingModel(modelName);
        }

        UpdatedAt = now;
        return this;
    }

    public ErrorOr<double> CalculateSimilarity(ProductImage other, string modelName)
    {
        var embeddingsResult = GetEmbeddingsPair(modelName, other);
        if (embeddingsResult.IsError) return embeddingsResult.Errors;

        var (selfEmb, otherEmb) = embeddingsResult.Value;
        
        // Calculate cosine similarity: 1 - cosine distance
        double cosineSimilarity = 1.0 - selfEmb.CosineDistance(otherEmb);
        
        // Normalize to [0, 1] range. Cosine sim is [-1, 1]. Map to [0, 1].
        double normalized = (cosineSimilarity + 1.0) / 2.0;
        
        return Math.Clamp(normalized, 0.0, 1.0);
    }

    private ErrorOr<(Vector self, Vector other)> GetEmbeddingsPair(string modelName, ProductImage other)
    {
        Vector? selfEmb = GetEmbeddingByModel(modelName);
        Vector? otherEmb = other.GetEmbeddingByModel(modelName);

        if (selfEmb == null || otherEmb == null) return Errors.EmbeddingNotGenerated(modelName);
        return (selfEmb, otherEmb);
    }

    private Vector? GetEmbeddingByModel(string modelName) =>
        modelName.ToLowerInvariant() switch
        {
            "efficientnet_b0" or "efficientnet" => EmbeddingEfficientnet,
            "convnext_tiny" or "convnext" => EmbeddingConvnext,
            "clip_vit_b16" or "clip" => EmbeddingClip,
            "fashion_clip" or "fclip" => EmbeddingFclip,
            "dinov2_vits14" or "dinov2" or "dino" => EmbeddingDino,
            _ => null
        };

    private ErrorOr<int> TryGetExpectedDimension(string modelName) =>
        modelName.ToLowerInvariant() switch
        {
            "efficientnet_b0" or "efficientnet" => 1280,
            "convnext_tiny" or "convnext" => 768,
            "clip_vit_b16" or "clip" => 512,
            "fashion_clip" or "fclip" => 512,
            "dinov2_vits14" or "dinov2" or "dino" => 384,
            _ => Errors.UnsupportedEmbeddingModel(modelName)
        };

    private static string ComputeChecksum(float[] embedding)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var embeddingBytes = embedding.SelectMany(BitConverter.GetBytes).ToArray();
        var hash = sha.ComputeHash(embeddingBytes);
        return Convert.ToHexString(hash)[..16];
    }

    #endregion

    #region Helpers

    public sealed record ImageSizeSpec(int? TargetWidth, int? TargetHeight, double? AspectRatio, bool AllowUpscale, bool CropToFit)
    {
        public bool IsFixedSize => TargetWidth.HasValue && TargetHeight.HasValue;
    }

    public static ImageSizeSpec GetSizeSpec(ProductImageType type)
    {
        return type switch
        {
            ProductImageType.Default => new ImageSizeSpec(720, null, null, true, false), // Demo Size (720px)
            ProductImageType.Square => new ImageSizeSpec(1024, 1024, 1d, false, true),
            ProductImageType.Thumbnail => new ImageSizeSpec(300, 300, 1d, false, true),
            ProductImageType.Gallery => new ImageSizeSpec(1600, null, null, false, false),
            ProductImageType.Search => new ImageSizeSpec(512, 512, 1d, true, true),      // AI Search Size
            _ => new ImageSizeSpec(null, null, null, true, false)
        };
    }

    #endregion
}

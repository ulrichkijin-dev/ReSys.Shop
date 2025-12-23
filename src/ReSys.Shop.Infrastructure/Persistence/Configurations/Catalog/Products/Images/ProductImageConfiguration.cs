using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Catalog.Products.Images;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Catalog.Products.Images;

public sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        #region Table
        builder.ToTable(name: Schema.ProductImages);
        #endregion

        #region Primary Key
        builder.HasKey(keyExpression: pi => pi.Id);
        #endregion

        #region Indexes
        builder.HasIndex(indexExpression: pi => pi.ProductId);
        builder.HasIndex(indexExpression: pi => pi.VariantId);
        builder.HasIndex(indexExpression: pi => pi.Position);
        #endregion

        #region Properties
        builder.Property(propertyExpression: pi => pi.Id)
           .ValueGeneratedNever()
           .HasComment(comment: "Id: Unique identifier for the product image.");

        builder.Property(propertyExpression: pi => pi.Url)
            .HasMaxLength(maxLength: ProductImage.Constraints.UrlMaxLength)
            .IsRequired()
            .HasComment(comment: "Url: The URL of the image asset.");

        builder.Property(propertyExpression: pi => pi.Alt)
            .HasMaxLength(maxLength: ProductImage.Constraints.AltMaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "Alt: Alternative text for the image.");

        builder.Property(propertyExpression: pi => pi.ContentType)
            .HasMaxLength(maxLength: 50)
            .IsRequired()
            .HasComment(comment: "ContentType: MIME type of the image.");

        builder.Property(propertyExpression: pi => pi.Width)
            .IsRequired(required: false)
            .HasComment(comment: "Width: Image width in pixels.");

        builder.Property(propertyExpression: pi => pi.Height)
            .IsRequired(required: false)
            .HasComment(comment: "Height: Image height in pixels.");

        builder.Property(propertyExpression: pi => pi.DimensionsUnit)
            .HasMaxLength(maxLength: 10)
            .IsRequired(required: false)
            .HasComment(comment: "DimensionsUnit: Unit of measurement.");

        builder.Property(propertyExpression: pi => pi.Type)
            .HasMaxLength(maxLength: 50)
            .IsRequired()
            .HasComment(comment: "Type: Image type (Default, Thumbnail, Gallery).");

        builder.Property(propertyExpression: pi => pi.ProductId)
            .IsRequired(required: false)
            .HasComment(comment: "ProductId: Foreign key to Product.");

        builder.Property(propertyExpression: pi => pi.VariantId)
            .IsRequired(required: false)
            .HasComment(comment: "VariantId: Foreign key to Variant.");

        #region Vector Embedding Configuration
        
        // Thesis Strategy Models
        
        // 1. Production Baseline CNN (EfficientNet B0)
        builder.Property(propertyExpression: pi => pi.EmbeddingEfficientnet)
            .HasColumnType(typeName: "vector(1280)")
            .IsRequired(required: false)
            .HasComment(comment: "EmbeddingEfficientnet: Embedding vector for EfficientNet B0 (1280-dim).");

        builder.Property(pi => pi.EmbeddingEfficientnetModel)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(pi => pi.EmbeddingEfficientnetGeneratedAt)
            .IsRequired(false);

        builder.Property(pi => pi.EmbeddingEfficientnetChecksum)
            .HasMaxLength(64)
            .IsRequired(false);

        builder.HasIndex(indexExpression: pi => pi.EmbeddingEfficientnet)
            .HasDatabaseName(name: "ix_product_images_embedding_efficientnet_hnsw")
            .HasMethod(method: "hnsw")
            .HasOperators(operators: "vector_cosine_ops");

        // 2. Modern CNN (ConvNeXt Tiny)
        builder.Property(propertyExpression: pi => pi.EmbeddingConvnext)
            .HasColumnType(typeName: "vector(768)")
            .IsRequired(required: false)
            .HasComment(comment: "EmbeddingConvnext: Embedding vector for ConvNeXt Tiny (768-dim).");

        builder.Property(pi => pi.EmbeddingConvnextModel)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(pi => pi.EmbeddingConvnextGeneratedAt)
            .IsRequired(false);

        builder.Property(pi => pi.EmbeddingConvnextChecksum)
            .HasMaxLength(64)
            .IsRequired(false);

        builder.HasIndex(indexExpression: pi => pi.EmbeddingConvnext)
            .HasDatabaseName(name: "ix_product_images_embedding_convnext_hnsw")
            .HasMethod(method: "hnsw")
            .HasOperators(operators: "vector_cosine_ops");

        // 3. CLIP ViT-B/16 (General Transformer)
        builder.Property(propertyExpression: pi => pi.EmbeddingClip)
            .HasColumnType(typeName: "vector(512)")
            .IsRequired(required: false)
            .HasComment(comment: "EmbeddingClip: Embedding vector for CLIP ViT-B/16 (512-dim).");

        builder.Property(pi => pi.EmbeddingClipModel)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(pi => pi.EmbeddingClipGeneratedAt)
            .IsRequired(false);

        builder.Property(pi => pi.EmbeddingClipChecksum)
            .HasMaxLength(64)
            .IsRequired(false);

        builder.HasIndex(indexExpression: pi => pi.EmbeddingClip)
            .HasDatabaseName(name: "ix_product_images_embedding_clip_hnsw")
            .HasMethod(method: "hnsw")
            .HasOperators(operators: "vector_cosine_ops");

        // 4. Semantic Transformer (Fashion-CLIP)
        builder.Property(propertyExpression: pi => pi.EmbeddingFclip)
            .HasColumnType(typeName: "vector(512)")
            .IsRequired(required: false)
            .HasComment(comment: "EmbeddingFclip: Embedding vector for Fashion-CLIP (512-dim).");

        builder.Property(pi => pi.EmbeddingFclipModel)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(pi => pi.EmbeddingFclipGeneratedAt)
            .IsRequired(false);

        builder.Property(pi => pi.EmbeddingFclipChecksum)
            .HasMaxLength(64)
            .IsRequired(false);

        builder.HasIndex(indexExpression: pi => pi.EmbeddingFclip)
            .HasDatabaseName(name: "ix_product_images_embedding_fclip_hnsw")
            .HasMethod(method: "hnsw")
            .HasOperators(operators: "vector_cosine_ops");

        // 5. Visual Structure Transformer (DINOv2 ViT-S/14)
        builder.Property(propertyExpression: pi => pi.EmbeddingDino)
            .HasColumnType(typeName: "vector(384)")
            .IsRequired(required: false)
            .HasComment(comment: "EmbeddingDino: Embedding vector for DINOv2 ViT-S/14 (384-dim).");

        builder.Property(pi => pi.EmbeddingDinoModel)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(pi => pi.EmbeddingDinoGeneratedAt)
            .IsRequired(false);

        builder.Property(pi => pi.EmbeddingDinoChecksum)
            .HasMaxLength(64)
            .IsRequired(false);

        builder.HasIndex(indexExpression: pi => pi.EmbeddingDino)
            .HasDatabaseName(name: "ix_product_images_embedding_dino_hnsw")
            .HasMethod(method: "hnsw")
            .HasOperators(operators: "vector_cosine_ops");

        #endregion

        builder.ConfigurePosition();
        builder.ConfigureMetadata();
        builder.ConfigureAuditable();
        #endregion

        #region Relationships
        builder.HasOne(navigationExpression: pi => pi.Product)
            .WithMany(navigationExpression: p => p.Images)
            .HasForeignKey(foreignKeyExpression: pi => pi.ProductId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade)
            .IsRequired(required: false);

        builder.HasOne(navigationExpression: pi => pi.Variant)
            .WithMany(navigationExpression: v => v.Images)
            .HasForeignKey(foreignKeyExpression: pi => pi.VariantId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade)
            .IsRequired(required: false);
        #endregion
    }
}
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
        
        // Thesis Comparison Models
        
        // 1. Efficient CNN (MobileNetV3)
        builder.Property(propertyExpression: pi => pi.EmbeddingMobilenet)
            .HasColumnType(typeName: "vector(576)")
            .IsRequired(required: false)
            .HasComment(comment: "EmbeddingMobilenet: Embedding vector for MobileNetV3 (576-dim).");

        builder.HasIndex(indexExpression: pi => pi.EmbeddingMobilenet)
            .HasDatabaseName(name: "ix_product_images_embedding_mobilenet_hnsw")
            .HasMethod(method: "hnsw")
            .HasOperators(operators: "vector_cosine_ops");

        // 2. Scaled CNN (EfficientNet B0)
        builder.Property(propertyExpression: pi => pi.EmbeddingEfficientnet)
            .HasColumnType(typeName: "vector(1280)")
            .IsRequired(required: false)
            .HasComment(comment: "EmbeddingEfficientnet: Embedding vector for EfficientNet B0 (1280-dim).");

        builder.HasIndex(indexExpression: pi => pi.EmbeddingEfficientnet)
            .HasDatabaseName(name: "ix_product_images_embedding_efficientnet_hnsw")
            .HasMethod(method: "hnsw")
            .HasOperators(operators: "vector_cosine_ops");

        // 3. Transformer (CLIP ViT-B/32)
        builder.Property(propertyExpression: pi => pi.EmbeddingClip)
            .HasColumnType(typeName: "vector(512)")
            .IsRequired(required: false)
            .HasComment(comment: "EmbeddingClip: Embedding vector for CLIP ViT-B/32 (512-dim).");

        builder.HasIndex(indexExpression: pi => pi.EmbeddingClip)
            .HasDatabaseName(name: "ix_product_images_embedding_clip_hnsw")
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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Common.Shared;
using ReSys.Shop.Core.Domain.Catalog.Taxonomies.Images;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Catalog.Taxonomies.Images;

/// <summary>
/// Configures the database mapping for the <see cref="TaxonImage"/> entity.
/// </summary>
public sealed class TaxonImageConfiguration : IEntityTypeConfiguration<TaxonImage>
{
    /// <summary>
    /// Configures the entity of type <see cref="TaxonImage"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<TaxonImage> builder)
    {
        #region Table

        builder.ToTable(name: Schema.TaxonImages);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: ti => ti.Id);
        #endregion

        #region Indexes

        builder.HasIndex(indexExpression: ti => ti.TaxonId);
        builder.HasIndex(indexExpression: ti => ti.Type);
        #endregion

        #region Properties

        builder.Property(propertyExpression: ti => ti.Id)
            .ValueGeneratedNever()
            .HasComment(comment: "Id: Unique identifier for the taxon image. Value generated never.");

        builder.Property(propertyExpression: ti => ti.TaxonId)
            .IsRequired()
            .HasComment(comment: "TaxonId: Foreign key to the associated Taxon.");

        builder.Property(propertyExpression: ti => ti.Type)
            .ConfigureShortText()
            .HasComment(comment: "Type: The type of the image (e.g., 'default', 'square').");
        
        builder.Property(propertyExpression: pi => pi.Alt)
            .ConfigureTitleOptional()
            .HasComment(comment: "Alt: Alternative text for the image.");

        builder.Property(propertyExpression: ti => ti.Url)
            .ConfigureUrlOptional()
            .HasComment(comment: "Url: The URL of the image asset.");

        builder.ConfigurePosition();
        builder.ConfigureMetadata();
        builder.ConfigureAuditable();
        #endregion

        #region Relationships

        builder.HasOne(navigationExpression: ti => ti.Taxon)
            .WithMany(navigationExpression: t => t.TaxonImages)
            .HasForeignKey(foreignKeyExpression: ti => ti.TaxonId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
        #endregion

    }
}

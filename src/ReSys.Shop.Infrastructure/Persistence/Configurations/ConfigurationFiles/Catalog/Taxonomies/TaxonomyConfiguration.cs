using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Catalog.Taxonomies;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.ConfigurationFiles.Catalog.Taxonomies;

/// <summary>
/// Configures the database mapping for the <see cref="Taxonomy"/> entity.
/// </summary>
public sealed class TaxonomyConfiguration : IEntityTypeConfiguration<Taxonomy>
{
    /// <summary>
    /// Configures the entity of type <see cref="Taxonomy"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<Taxonomy> builder)
    {
        #region Table

        builder.ToTable(name: Schema.Taxonomies);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: t => t.Id);
        #endregion

        #region Indexes

        builder.HasIndex(indexExpression: t => t.Name).IsUnique();
        builder.HasIndex(indexExpression: t => t.Position);
        #endregion

        #region Properties

        builder.Property(propertyExpression: t => t.Id)
            .HasColumnName(name: "id")
            .ValueGeneratedNever()
            .HasComment(comment: "Id: Unique identifier for the taxonomy. Value generated never.");

        builder.ConfigureParameterizableName();
        builder.ConfigurePosition();
        builder.ConfigureMetadata();
        builder.ConfigureAuditable();
        builder.ConfigureUniqueName();
        #endregion

        #region Relationships
        builder.HasMany(navigationExpression: t => t.Taxons)
            .WithOne(navigationExpression: tx => tx.Taxonomy)
            .HasForeignKey(foreignKeyExpression: tx => tx.TaxonomyId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);

        #endregion

        #region Concurrency
        builder.ConfigureVersion();
        #endregion
    }
}

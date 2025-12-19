using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Catalog.Products.Variants;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.ConfigurationFiles.Catalog.Products.Variants;

/// <summary>
/// Configures the database mapping for the <see cref="VariantOptionValue"/> entity.
/// </summary>
public sealed class VariantOptionValueConfiguration : IEntityTypeConfiguration<VariantOptionValue>
{
    /// <summary>
    /// Configures the entity of type <see cref="VariantOptionValue"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<VariantOptionValue> builder)
    {
        #region Table

        builder.ToTable(name: Schema.VariantOptionValues);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: ovv => ovv.Id);
        #endregion

        #region Indexes

        builder.HasIndex(indexExpression: ovv => ovv.VariantId);
        builder.HasIndex(indexExpression: ovv => ovv.OptionValueId);
        builder.HasIndex(indexExpression: ovv => new { ovv.VariantId, ovv.OptionValueId }).IsUnique();
        #endregion

        #region Properties

        builder.Property(propertyExpression: ovv => ovv.Id)
            .HasComment(comment: "Id: Unique identifier for the option value variant. Value generated never.");

        builder.Property(propertyExpression: ovv => ovv.VariantId)
            .IsRequired()
            .HasComment(comment: "VariantId: Foreign key to the associated Product Variant.");

        builder.Property(propertyExpression: ovv => ovv.OptionValueId)
            .IsRequired()
            .HasComment(comment: "OptionValueId: Foreign key to the associated OptionValue.");

        builder.ConfigureAuditable();

        #endregion

        #region Relationships

        builder.HasOne(navigationExpression: ovv => ovv.Variant)
            .WithMany(navigationExpression: v => v.VariantOptionValues)
            .HasForeignKey(foreignKeyExpression: ovv => ovv.VariantId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);

        builder.HasOne(navigationExpression: ovv => ovv.OptionValue)
            .WithMany(navigationExpression: ov => ov.VariantOptionValues)
            .HasForeignKey(foreignKeyExpression: ovv => ovv.OptionValueId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
        #endregion
    }
}

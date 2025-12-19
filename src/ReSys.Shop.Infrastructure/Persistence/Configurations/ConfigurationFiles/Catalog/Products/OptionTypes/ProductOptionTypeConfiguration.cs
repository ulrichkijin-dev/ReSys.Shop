using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Catalog.Products.OptionTypes;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.ConfigurationFiles.Catalog.Products.OptionTypes;
/// <summary>
/// Configures the database mapping for the <see cref="ProductOptionType"/> entity.
/// </summary>
internal class ProductOptionTypeConfiguration : IEntityTypeConfiguration<ProductOptionType>
{
    /// <summary>
    /// Configures the entity of type <see cref="ProductOptionType"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<ProductOptionType> builder)
    {
        #region Table

        builder.ToTable(name: Schema.ProductOptionTypes);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: pot => pot.Id);
        #endregion

        #region Indexes

        builder.HasIndex(indexExpression: pot => pot.ProductId);
        builder.HasIndex(indexExpression: pot => pot.OptionTypeId);
        builder.HasIndex(indexExpression: pot => new { pot.ProductId, pot.OptionTypeId }).IsUnique();
        builder.HasIndex(indexExpression: pot => pot.Position);

        #endregion

        #region Properties

        builder.Property(propertyExpression: pot => pot.Id)
            .HasComment(comment: "Id: Unique identifier for the product option type. Value generated never.");

        builder.Property(propertyExpression: pot => pot.ProductId)
            .IsRequired()
            .HasComment(comment: "ProductId: Foreign key to the associated Product.");

        builder.Property(propertyExpression: pot => pot.OptionTypeId)
            .IsRequired()
            .HasComment(comment: "OptionTypeId: Foreign key to the associated OptionType.");

        builder.Property(propertyExpression: pot => pot.Position)
            .IsRequired()
            .HasComment(comment: "Position: The display order of the option type for the product.");

        builder.ConfigureAuditable();
        builder.ConfigurePosition();
        #endregion

        #region Relationships

        builder.HasOne(navigationExpression: pot => pot.Product)
            .WithMany(navigationExpression: p => p.ProductOptionTypes)
            .HasForeignKey(foreignKeyExpression: pot => pot.ProductId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasOne(navigationExpression: pot => pot.OptionType)
            .WithMany(navigationExpression: ot => ot.ProductOptionTypes)
            .HasForeignKey(foreignKeyExpression: pot => pot.OptionTypeId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade)
            .IsRequired();
        #endregion
    }
}

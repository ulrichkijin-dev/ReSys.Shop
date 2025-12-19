using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Common.Shared;
using ReSys.Shop.Core.Domain.Catalog.Products.Prices;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.ConfigurationFiles.Catalog.Products.Prices;

/// <summary>
/// Configures the database mapping for the <see cref="Price"/> entity.
/// </summary>
public sealed class PriceConfiguration : IEntityTypeConfiguration<Price>
{
    /// <summary>
    /// Configures the entity of type <see cref="Price"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<Price> builder)
    {
        #region Table

        builder.ToTable(name: Schema.Prices);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: p => p.Id);
        #endregion

        #region Indexes

        builder.HasIndex(indexExpression: p => p.VariantId);
        builder.HasIndex(indexExpression: p => p.Currency);
        #endregion

        #region Properties

        builder.Property(propertyExpression: p => p.Id)
            .HasColumnName(name: "id")
            .HasComment(comment: "Id: Unique identifier for the price. Value generated never.");

        builder.Property(propertyExpression: p => p.VariantId)
            .IsRequired()
            .HasComment(comment: "VariantId: Foreign key to the associated Product Variant.");

        builder.Property(propertyExpression: p => p.Amount)
            .HasColumnType(typeName: "decimal(18,4)")
            .IsRequired(required: false)
            .HasComment(comment: "Amount: The current price of the product variant.");

        builder.Property(propertyExpression: p => p.Currency)
            .HasMaxLength(maxLength: CommonInput.Constraints.CurrencyAndLanguage.CurrencyCodeLength)
            .IsRequired()
            .HasComment(comment: "Currency: The currency of the price (e.g., 'USD', 'EUR').");

        builder.Property(propertyExpression: p => p.CompareAtAmount)
            .HasColumnType(typeName: "decimal(18,4)")
            .IsRequired(required: false)
            .HasComment(comment: "CompareAtAmount: The original price for comparison, indicating a sale or discount.");

        builder.ConfigureAuditable();
        #endregion

        #region Relationships

        builder.HasOne(navigationExpression: p => p.Variant)
            .WithMany(navigationExpression: v => v.Prices)
            .HasForeignKey(foreignKeyExpression: p => p.VariantId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
        #endregion

        #region Concurrency
        builder.ConfigureVersion();
        #endregion
    }
}

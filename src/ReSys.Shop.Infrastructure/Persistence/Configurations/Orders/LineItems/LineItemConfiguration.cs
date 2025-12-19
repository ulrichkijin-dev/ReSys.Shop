using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Orders.LineItems;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Orders.LineItems;

/// <summary>
/// Configures the database mapping for the <see cref="LineItem"/> entity.
/// </summary>
public sealed class LineItemConfiguration : IEntityTypeConfiguration<LineItem>
{
    /// <summary>
    /// Configures the entity of type <see cref="LineItem"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<LineItem> builder)
    {
        #region Table

        builder.ToTable(name: Schema.LineItems);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: li => li.Id);
        #endregion

        #region Properties

        builder.Property(propertyExpression: li => li.Id)
            .HasColumnName(name: "id")
            .ValueGeneratedNever()
            .HasComment(comment: "Id: Unique identifier for the line item. Value generated never.");

        builder.Property(propertyExpression: li => li.OrderId)
            .IsRequired()
            .HasComment(comment: "OrderId: Foreign key to the associated Order.");

        builder.Property(propertyExpression: li => li.VariantId)
            .IsRequired()
            .HasComment(comment: "VariantId: Foreign key to the associated Product Variant.");

        builder.Property(propertyExpression: li => li.Quantity)
            .IsRequired()
            .HasComment(comment: "Quantity: Number of units of the product variant.");

        builder.Property(propertyExpression: li => li.PriceCents)
            .IsRequired()
            .HasComment(comment: "PriceCents: Price of a single unit in cents at the time of order.");

        builder.Property(propertyExpression: li => li.Currency)
            .HasMaxLength(maxLength: LineItem.Constraints.CurrencyMaxLength)
            .IsRequired()
            .HasComment(comment: "Currency: The currency of the line item.");

        builder.Property(propertyExpression: li => li.CapturedName)
            .HasMaxLength(maxLength: LineItem.Constraints.CapturedNameMaxLength)
            .IsRequired()
            .HasComment(comment: "CapturedName: Name of the product variant at the time of order.");

        builder.Property(propertyExpression: li => li.CapturedSku)
            .HasMaxLength(maxLength: LineItem.Constraints.CapturedSkuMaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "CapturedSku: SKU of the product variant at the time of order.");

        builder.Property(propertyExpression: li => li.IsPromotional)
            .IsRequired()
            .HasDefaultValue(value: false)
            .HasComment(comment: "IsPromotional: Indicates if this line item was part of a promotion.");

        builder.ConfigureAuditable();
        #endregion

        #region Relationships

        builder.HasOne(navigationExpression: li => li.Order)
            .WithMany(navigationExpression: o => o.LineItems)
            .HasForeignKey(foreignKeyExpression: li => li.OrderId)
            .OnDelete(deleteBehavior: DeleteBehavior.Restrict);

        builder.HasOne(navigationExpression: li => li.Variant)
            .WithMany(navigationExpression: v => v.LineItems)
            .HasForeignKey(foreignKeyExpression: li => li.VariantId)
            .OnDelete(deleteBehavior: DeleteBehavior.Restrict);

        #endregion

        #region Indexes

        builder.HasIndex(indexExpression: li => li.OrderId);
        builder.HasIndex(indexExpression: li => li.VariantId);
        #endregion

        #region Ignored Properties

        builder.Ignore(propertyExpression: li => li.SubtotalCents);
        builder.Ignore(propertyExpression: li => li.Subtotal);
        builder.Ignore(propertyExpression: li => li.UnitPrice);
        builder.Ignore(propertyExpression: li => li.TotalCents);
        builder.Ignore(propertyExpression: li => li.Total);
        builder.Ignore(propertyExpression: li => li.InventoryUnits);
        #endregion
    }
}

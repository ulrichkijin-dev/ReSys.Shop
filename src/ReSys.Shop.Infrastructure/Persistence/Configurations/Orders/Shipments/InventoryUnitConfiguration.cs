using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Domain.Orders.Shipments;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Orders.Shipments;

/// <summary>
/// Entity Framework Core configuration for InventoryUnit entity.
/// </summary>
public sealed class InventoryUnitConfiguration : IEntityTypeConfiguration<InventoryUnit>
{
    public void Configure(EntityTypeBuilder<InventoryUnit> builder)
    {
        #region Table

        builder.ToTable(name: Schema.InventoryUnits);
        #endregion

        builder.HasKey(keyExpression: e => e.Id);

        builder.Property(propertyExpression: e => e.Id)
            .ValueGeneratedNever();

        builder.Property(propertyExpression: e => e.VariantId)
            .IsRequired();

        builder.Property(propertyExpression: e => e.LineItemId)
            .IsRequired();

        builder.Property(propertyExpression: e => e.ShipmentId)
            .IsRequired(required: false);

        builder.Property(propertyExpression: e => e.State)
            .HasConversion<int>()
            .IsRequired();
            
        builder.Property(propertyExpression: e => e.StateChangedAt)
            .IsRequired();

        builder.Property(propertyExpression: e => e.RowVersion)
            .IsRowVersion();

        builder.Property(propertyExpression: e => e.CreatedAt).IsRequired();
        builder.Property(propertyExpression: e => e.UpdatedAt).IsRequired(required: false);

        builder.HasOne(navigationExpression: e => e.Variant)
            .WithMany()
            .HasForeignKey(foreignKeyExpression: e => e.VariantId)
            .IsRequired(required: false)
            .OnDelete(deleteBehavior: DeleteBehavior.NoAction);

        builder.HasOne(navigationExpression: e => e.LineItem)
            .WithMany()
            .HasForeignKey(foreignKeyExpression: e => e.LineItemId)
            .IsRequired()
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);

        builder.HasOne(navigationExpression: e => e.Shipment)
            .WithMany(navigationExpression: s => s.InventoryUnits)
            .HasForeignKey(foreignKeyExpression: e => e.ShipmentId)
            .IsRequired(required: false)
            .OnDelete(deleteBehavior: DeleteBehavior.SetNull);

        builder.HasIndex(indexExpression: e => e.LineItemId);
        builder.HasIndex(indexExpression: e => e.VariantId);
        builder.HasIndex(indexExpression: e => e.ShipmentId);
        builder.HasIndex(indexExpression: e => e.State);
        builder.HasIndex(indexExpression: e => new { e.VariantId, e.State });
        
        #region Ignored Properties
        builder.Ignore(propertyExpression: s => s.Order);
        builder.Ignore(propertyExpression: s => s.OrderId);
        builder.Ignore(propertyExpression: s => s.StockLocation);
        builder.Ignore(propertyExpression: s => s.StockLocationId);
        builder.Ignore(propertyExpression: s => s.IsPreShipment);
        builder.Ignore(propertyExpression: s => s.IsPostShipment);
        builder.Ignore(propertyExpression: s => s.IsCancelable);
        builder.Ignore(propertyExpression: s => s.IsShippable);
        builder.Ignore(propertyExpression: s => s.IsBackordered);
        builder.Ignore(propertyExpression: s => s.IsShipped);
        builder.Ignore(propertyExpression: s => s.IsCanceled);
        builder.Ignore(propertyExpression: s => s.AllowShip);
        #endregion
    }
}

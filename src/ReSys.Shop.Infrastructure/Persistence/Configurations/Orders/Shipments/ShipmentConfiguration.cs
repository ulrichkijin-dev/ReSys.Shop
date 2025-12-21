using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Orders.Shipments;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Orders.Shipments;

/// <summary>
/// Configures the database mapping for the <see cref="Shipment"/> entity.
/// </summary>
public sealed class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    /// <summary>
    /// Configures the entity of type <see cref="Shipment"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        #region Table
        builder.ToTable(name: Schema.Shipments);
        #endregion

        #region Primary Key
        builder.HasKey(keyExpression: s => s.Id);
        #endregion

        #region Properties
        builder.Property(propertyExpression: s => s.Id)
            .HasColumnName(name: "id")
            .ValueGeneratedNever();

        builder.Property(propertyExpression: s => s.OrderId)
            .IsRequired();

        builder.Property(propertyExpression: s => s.Number)
            .HasMaxLength(maxLength: Shipment.Constraints.NumberMaxLength)
            .IsRequired();

        builder.Property(propertyExpression: s => s.State)
            .HasConversion<string>()
            .HasMaxLength(maxLength: 50)
            .IsRequired();

        builder.Property(propertyExpression: s => s.TrackingNumber)
            .HasMaxLength(maxLength: Shipment.Constraints.TrackingNumberMaxLength)
            .IsRequired(required: false);

        builder.Property(propertyExpression: p => p.PackageId)
            .HasMaxLength(maxLength: Shipment.Constraints.PackageIdMaxLength)
            .IsRequired(required: false);
            
        builder.Property(propertyExpression: s => s.AllocatedAt).IsRequired(required: false);
        builder.Property(propertyExpression: s => s.PickingStartedAt).IsRequired(required: false);
        builder.Property(propertyExpression: s => s.PickedAt).IsRequired(required: false);
        builder.Property(propertyExpression: s => s.PackedAt).IsRequired(required: false);
        builder.Property(propertyExpression: s => s.ReadyToShipAt).IsRequired(required: false);
        builder.Property(propertyExpression: s => s.ShippedAt).IsRequired(required: false);
        builder.Property(propertyExpression: s => s.DeliveredAt).IsRequired(required: false);

        builder.Property(propertyExpression: s => s.StockLocationId)
            .IsRequired();

        builder.Property(propertyExpression: e => e.RowVersion).IsRowVersion();
        
        builder.ConfigureAuditable();
        #endregion

        #region Relationships
        builder.HasOne(navigationExpression: s => s.Order)
            .WithMany(navigationExpression: o => o.Shipments)
            .HasForeignKey(foreignKeyExpression: s => s.OrderId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);

        builder.HasOne(navigationExpression: s => s.StockLocation)
            .WithMany()
            .HasForeignKey(foreignKeyExpression: s => s.StockLocationId)
            .IsRequired()
            .OnDelete(deleteBehavior: DeleteBehavior.Restrict);

        builder.HasMany(navigationExpression: s => s.InventoryUnits)
            .WithOne(navigationExpression: iu => iu.Shipment)
            .HasForeignKey(foreignKeyExpression: iu => iu.ShipmentId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
            
        builder.HasMany(navigationExpression: s => s.StockMovements)
            .WithOne()
            .HasForeignKey(foreignKeyExpression: sm => sm.OriginatorId)
            .IsRequired(required: false)
            .OnDelete(deleteBehavior: DeleteBehavior.SetNull);
        #endregion

        #region Indexes
        builder.HasIndex(indexExpression: s => s.OrderId);
        builder.HasIndex(indexExpression: s => s.StockLocationId);
        builder.HasIndex(indexExpression: s => s.Number).IsUnique();
        builder.HasIndex(indexExpression: s => s.State);
        #endregion

        #region Ignored Properties
        builder.Ignore(propertyExpression: s => s.IsShipped);
        builder.Ignore(propertyExpression: s => s.IsDelivered);
        builder.Ignore(propertyExpression: s => s.IsCanceled);
        builder.Ignore(propertyExpression: s => s.IsPending);
        builder.Ignore(propertyExpression: s => s.IsReady);
        #endregion
    }
}
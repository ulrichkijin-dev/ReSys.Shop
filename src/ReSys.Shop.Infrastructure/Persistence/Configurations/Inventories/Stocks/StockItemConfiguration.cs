using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Inventories.Stocks;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Inventories.Stocks;

/// <summary>
/// Configures the database mapping for the <see cref="StockItem"/> entity.
/// </summary>
public sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    /// <summary>
    /// Configures the entity of type <see cref="StockItem"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        #region Table

        builder.ToTable(name: Schema.StockItems);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: si => si.Id);
        #endregion

        #region Properties

        builder.Property(propertyExpression: si => si.Id)
            .HasColumnName(name: "id")
            .ValueGeneratedNever()
            .HasComment(comment: "Unique identifier for the stock item.");

        builder.Property(propertyExpression: e => e.RowVersion)
            .IsRowVersion()
            .HasComment(comment: "RowVersion: Used for optimistic concurrency control.");

        builder.Property(propertyExpression: si => si.StockLocationId)
            .IsRequired()
            .HasComment(comment: "Foreign key to the associated StockLocation.");

        builder.Property(propertyExpression: si => si.VariantId)
            .IsRequired()
            .HasComment(comment: "Foreign key to the associated Variant.");

        builder.Property(propertyExpression: si => si.QuantityOnHand)
            .IsRequired()
            .HasDefaultValue(value: 0)
            .HasComment(comment: "QuantityOnHand: The current quantity of the variant in stock.");

        builder.Property(propertyExpression: si => si.QuantityReserved)
            .IsRequired()
            .HasDefaultValue(value: 0)
            .HasComment(comment: "QuantityReserved: The quantity of the variant reserved for orders.");

        builder.Property(propertyExpression: si => si.Backorderable)
            .IsRequired()
            .HasDefaultValue(value: true)
            .HasComment(comment: "Backorderable: Indicates if the variant can be backordered.");

        builder.ConfigureAuditable();
        builder.ConfigureMetadata();

        #endregion

        #region Relationships

        builder.HasOne(navigationExpression: si => si.StockLocation)
            .WithMany(navigationExpression: sl => sl.StockItems)
            .HasForeignKey(foreignKeyExpression: si => si.StockLocationId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);

        builder.HasOne(navigationExpression: si => si.Variant)
            .WithMany(navigationExpression: v => v.StockItems)
            .HasForeignKey(foreignKeyExpression: si => si.VariantId)
            .OnDelete(deleteBehavior: DeleteBehavior.SetNull);

        builder.HasMany(navigationExpression: si => si.StockMovements)
            .WithOne(navigationExpression: sm => sm.StockItem)
            .HasForeignKey(foreignKeyExpression: sm => sm.StockItemId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);

        #endregion

        #region Indexes

        builder.HasIndex(indexExpression: si => new { si.StockLocationId, si.VariantId }).IsUnique();
        builder.HasIndex(indexExpression: si => si.VariantId);
        builder.HasIndex(indexExpression: si => si.StockLocationId);
        builder.HasIndex(indexExpression: si => new { si.QuantityReserved, si.QuantityOnHand });
        #endregion

        #region Concurrency
        builder.ConfigureVersion();
        #endregion

        #region Ignored Properties

        builder.Ignore(propertyExpression: si => si.CountAvailable);
        builder.Ignore(propertyExpression: si => si.InStock);
        #endregion
    }
}
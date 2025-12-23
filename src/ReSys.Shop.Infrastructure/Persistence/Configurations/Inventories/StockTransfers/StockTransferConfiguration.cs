using ReSys.Shop.Core.Common.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Shop.Core.Domain.Inventories.StockTransfers;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Inventories.StockTransfers;

/// <summary>
/// Configures the database mapping for the <see cref="StockTransfer"/> entity.
/// </summary>
public class StockTransferConfiguration : IEntityTypeConfiguration<StockTransfer>
{
    /// <summary>
    /// Configures the entity of type <see cref="StockTransfer"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        builder.ToTable(name: Schema.StockTransfers);

        builder.HasKey(keyExpression: st => st.Id);

        builder.Property(propertyExpression: st => st.Id)
            .ValueGeneratedNever()
            .HasComment(comment: "Unique identifier for the stock transfer.");

        builder.Property(propertyExpression: st => st.SourceLocationId)
            .IsRequired(required: false)
            .HasComment(comment: "Foreign key to the source location (null for supplier receipts).");

        builder.Property(propertyExpression: st => st.DestinationLocationId)
            .IsRequired()
            .HasComment(comment: "Foreign key to the destination location (required).");

        builder.Property(propertyExpression: st => st.Number)
            .HasMaxLength(maxLength: StockTransfer.Constraints.NumberMaxLength)
            .IsRequired()
            .HasComment(comment: "Number: Auto-generated transfer number for reference.");

        builder.Property(propertyExpression: st => st.Reference)
            .HasMaxLength(maxLength: StockTransfer.Constraints.ReferenceMaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "Reference: Optional reference code (e.g., purchase order number).");

        builder.Property(propertyExpression: st => st.State)
            .IsRequired()
            .HasConversion<int>()
            .HasComment(comment: "The current state of the stock transfer (e.g., Pending, Finalized).");

        builder.HasOne(navigationExpression: st => st.SourceLocation)
            .WithMany()
            .HasForeignKey(foreignKeyExpression: st => st.SourceLocationId)
            .OnDelete(deleteBehavior: DeleteBehavior.SetNull);

        builder.HasOne(navigationExpression: st => st.DestinationLocation)
            .WithMany()
            .HasForeignKey(foreignKeyExpression: st => st.DestinationLocationId)
            .OnDelete(deleteBehavior: DeleteBehavior.Restrict); // Prevent deleting location if transfers exist

        builder.HasIndex(indexExpression: st => st.SourceLocationId);
        builder.HasIndex(indexExpression: st => st.DestinationLocationId);
        builder.HasIndex(indexExpression: st => st.Number).IsUnique();
        builder.HasIndex(indexExpression: st => st.Reference);
        builder.HasIndex(indexExpression: st => st.CreatedAt);
 }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Orders.History;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Orders.History;

/// <summary>
/// Configures the database mapping for the <see cref="OrderHistory"/> entity.
/// </summary>
public sealed class OrderHistoryConfiguration : IEntityTypeConfiguration<OrderHistory>
{
    public void Configure(EntityTypeBuilder<OrderHistory> builder)
    {
        #region Table
        builder.ToTable(name: "order_history_logs");
        #endregion

        #region Primary Key
        builder.HasKey(keyExpression: h => h.Id);
        #endregion

        #region Properties
        builder.Property(propertyExpression: h => h.Id)
            .ValueGeneratedNever();

        builder.Property(propertyExpression: h => h.OrderId)
            .IsRequired();

        builder.Property(propertyExpression: h => h.Description)
            .HasMaxLength(maxLength: OrderHistory.Constraints.DescriptionMaxLength)
            .IsRequired();

        builder.Property(propertyExpression: h => h.FromState)
            .HasConversion<string>()
            .HasMaxLength(maxLength: 50)
            .IsRequired(required: false);
        
        builder.Property(propertyExpression: h => h.ToState)
            .HasConversion<string>()
            .HasMaxLength(maxLength: 50)
            .IsRequired();
        
        builder.Property(propertyExpression: h => h.TriggeredBy)
            .HasMaxLength(maxLength: OrderHistory.Constraints.TriggeredByMaxLength)
            .IsRequired(required: false);

        builder.Property(propertyExpression: h => h.Context)
            .HasColumnType(typeName: "jsonb")
            .IsRequired(required: false);

        builder.ConfigureAuditable();
        #endregion

        #region Relationships
        builder.HasOne(navigationExpression: h => h.Order)
            .WithMany(navigationExpression: o => o.Histories)
            .HasForeignKey(foreignKeyExpression: h => h.OrderId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
        #endregion

        #region Indexes

        builder.HasIndex(indexExpression: h => h.OrderId);

        builder.HasIndex(indexExpression: h => h.ToState);
        #endregion
    }
}

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Orders.Adjustments;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Orders.Adjustments;

/// <summary>
/// Configures the database mapping for the <see cref="OrderAdjustment"/> entity.
/// </summary>
public sealed class OrderAdjustmentConfiguration : IEntityTypeConfiguration<OrderAdjustment>
{
    /// <summary>
    /// Configures the entity of type <see cref="OrderAdjustment"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<OrderAdjustment> builder)
    {
        #region Table

        builder.ToTable(name: Schema.OrderAdjustments);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: oa => oa.Id);
        #endregion

        #region Properties

        builder.Property(propertyExpression: oa => oa.Id)
            .HasColumnName(name: "id")
            .ValueGeneratedNever()
            .HasComment(comment: "Id: Unique identifier for the order adjustment. Value generated never.");

        builder.Property(propertyExpression: oa => oa.OrderId)
            .IsRequired()
            .HasComment(comment: "OrderId: Foreign key to the associated Order.");

        builder.Property(propertyExpression: oa => oa.PromotionId)
            .IsRequired(required: false)
            .HasComment(comment: "PromotionId: Foreign key to the associated Promotion.");

        builder.Property(propertyExpression: oa => oa.AmountCents)
            .IsRequired()
            .HasComment(comment: "AmountCents: The adjustment amount in cents.");

        builder.Property(propertyExpression: oa => oa.Description)
            .HasMaxLength(maxLength: OrderAdjustment.Constraints.DescriptionMaxLength)
            .IsRequired()
            .HasComment(comment: "Description: Description of the adjustment.");

        builder.ConfigureAuditable();
        #endregion

        #region Relationships

        builder.HasOne(navigationExpression: oa => oa.Order)
            .WithMany(navigationExpression: o => o.OrderAdjustments)
            .HasForeignKey(foreignKeyExpression: oa => oa.OrderId)
            .OnDelete(deleteBehavior: DeleteBehavior.Restrict);

        builder.HasOne(navigationExpression: oa => oa.Promotion)
            .WithMany(navigationExpression: p => p.PromotionOrderAdjustments)
            .HasForeignKey(foreignKeyExpression: oa => oa.PromotionId)
            .IsRequired(required: false)
            .OnDelete(deleteBehavior: DeleteBehavior.SetNull);

        #endregion

        #region Indexes

        builder.HasIndex(indexExpression: oa => oa.OrderId);
        builder.HasIndex(indexExpression: oa => oa.PromotionId);
        #endregion

        #region Ignored Properties

        builder.Ignore(propertyExpression: oa => oa.IsPromotion);
        #endregion
    }
}

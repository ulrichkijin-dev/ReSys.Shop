using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Orders.Adjustments;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Orders.Adjustments;

/// <summary>
/// Configures the database mapping for the <see cref="LineItemAdjustment"/> entity.
/// </summary>
public sealed class LineItemAdjustmentConfiguration : IEntityTypeConfiguration<LineItemAdjustment>
{
    /// <summary>
    /// Configures the entity of type <see cref="LineItemAdjustment"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<LineItemAdjustment> builder)
    {
        #region Table

        builder.ToTable(name: Schema.LineItemAdjustments);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: lia => lia.Id);
        #endregion

        #region Properties

        builder.Property(propertyExpression: lia => lia.Id)
            .HasColumnName(name: "id")
            .ValueGeneratedNever()
            .HasComment(comment: "Id: Unique identifier for the line item adjustment. Value generated never.");

        builder.Property(propertyExpression: lia => lia.LineItemId)
            .IsRequired()
            .HasComment(comment: "LineItemId: Foreign key to the associated LineItem.");

        builder.Property(propertyExpression: lia => lia.PromotionId)
            .IsRequired(required: false)
            .HasComment(comment: "PromotionId: Foreign key to the associated Promotion.");

        builder.Property(propertyExpression: lia => lia.AmountCents)
            .IsRequired()
            .HasComment(comment: "AmountCents: The adjustment amount in cents.");

        builder.Property(propertyExpression: lia => lia.Description)
            .HasMaxLength(maxLength: LineItemAdjustment.Constraints.DescriptionMaxLength)
            .IsRequired()
            .HasComment(comment: "Description: Description of the adjustment.");

        builder.ConfigureAuditable();
        #endregion

        #region Relationships

        builder.HasOne(navigationExpression: lia => lia.LineItem)
            .WithMany(navigationExpression: li => li.Adjustments)
            .HasForeignKey(foreignKeyExpression: lia => lia.LineItemId)
            .OnDelete(deleteBehavior: DeleteBehavior.Restrict);

        builder.HasOne(navigationExpression: lia => lia.Promotion)
            .WithMany(navigationExpression: p => p.LineItemAdjustments)
            .HasForeignKey(foreignKeyExpression: lia => lia.PromotionId)
            .IsRequired(required: false)
            .OnDelete(deleteBehavior: DeleteBehavior.SetNull);

        #endregion

        #region Indexes

        builder.HasIndex(indexExpression: lia => lia.LineItemId);
        builder.HasIndex(indexExpression: lia => lia.PromotionId);
        #endregion

        #region Ignored Properties

        builder.Ignore(propertyExpression: lia => lia.IsPromotion);
        #endregion
    }
}
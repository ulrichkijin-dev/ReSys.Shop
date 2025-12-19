using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Common.Shared;
using ReSys.Shop.Core.Domain.Promotions.Actions;
using ReSys.Shop.Core.Domain.Promotions.Promotions;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Promotions.Promotions;

/// <summary>
/// Configures the database mapping for the <see cref="Promotion"/> entity.
/// </summary>
/// <summary>
/// Configures the database mapping for the <see cref="Promotion"/> entity.
/// </summary>
public sealed class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    /// <summary>
    /// Configures the entity of type <see cref="Promotion"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        #region Table

        builder.ToTable(name: Schema.Promotions);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: p => p.Id);
        #endregion

        #region Indexes

        builder.HasIndex(indexExpression: p => p.PromotionCode);

        builder.HasIndex(indexExpression: p => p.Active);
        builder.HasIndex(indexExpression: p => new { p.StartsAt, p.ExpiresAt, p.Active });

        #endregion

        #region Properties

        builder.Property(propertyExpression: p => p.Id)
            .HasColumnName(name: "id")
            .ValueGeneratedNever()
            .HasComment(comment: "Id: Unique identifier for the promotion.");

        builder.ConfigureUniqueName();

        builder.Property(propertyExpression: p => p.PromotionCode)
            .HasMaxLength(maxLength: Promotion.Constraints.CodeMaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "Code: Optional coupon code for the promotion.");

        builder.Property(propertyExpression: p => p.Description)
            .ConfigureDescriptionOptional(isRequired: false)
            .HasComment(comment: "Description: Optional detailed description of the promotion.");

        builder.Property(propertyExpression: p => p.MinimumOrderAmount)
            .HasPrecision(precision: 18, scale: 4)
            .IsRequired(required: false)
            .HasComment(comment: "MinimumOrderAmount: Minimum order amount required for the promotion to apply.");

        builder.Property(propertyExpression: p => p.MaximumDiscountAmount)
            .HasPrecision(precision: 18, scale: 4)
            .IsRequired(required: false)
            .HasComment(comment: "MaximumDiscountAmount: Maximum discount amount that can be applied by the promotion.");

        builder.Property(propertyExpression: p => p.StartsAt)
            .IsRequired(required: false)
            .HasComment(comment: "StartsAt: Optional start date/time when the promotion becomes active.");

        builder.Property(propertyExpression: p => p.ExpiresAt)
            .IsRequired(required: false)
            .HasComment(comment: "ExpiresAt: Optional expiration date/time when the promotion ends.");

        builder.Property(propertyExpression: p => p.UsageLimit)
            .IsRequired(required: false)
            .HasComment(comment: "UsageLimit: Optional maximum number of times the promotion can be used.");

        builder.Property(propertyExpression: p => p.UsageCount)
            .IsRequired()
            .HasDefaultValue(value: 0)
            .HasComment(comment: "UsageCount: Number of times the promotion has been used.");

        builder.Property(propertyExpression: p => p.Active)
            .IsRequired()
            .HasDefaultValue(value: true)
            .HasComment(comment: "Active: Indicates if the promotion is manually activated/deactivated.");

        builder.Property(propertyExpression: p => p.RequiresCouponCode)
            .IsRequired()
            .HasDefaultValue(value: false)
            .HasComment(comment: "RequiresCouponCode: Indicates if a coupon code must be entered to use this promotion.");

        builder.HasOne(navigationExpression: p => p.Action)
            .WithOne(navigationExpression: a => a.Promotion)
            .HasForeignKey<PromotionAction>(foreignKeyExpression: a => a.PromotionId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);


        builder.ConfigureAuditable();
        #endregion

        #region Relationships

        builder.HasMany(navigationExpression: p => p.PromotionRules)
            .WithOne(navigationExpression: r => r.Promotion)
            .HasForeignKey(foreignKeyExpression: r => r.PromotionId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);

        builder.HasMany(navigationExpression: p => p.Orders)
            .WithOne(navigationExpression: o => o.Promotion)
            .HasForeignKey(foreignKeyExpression: o => o.PromotionId)
            .IsRequired(required: false)
            .OnDelete(deleteBehavior: DeleteBehavior.SetNull);

        #endregion

        #region Ignored Properties

        builder.Ignore(propertyExpression: p => p.Type);
        builder.Ignore(propertyExpression: p => p.IsActive);
        builder.Ignore(propertyExpression: p => p.IsExpired);
        builder.Ignore(propertyExpression: p => p.HasUsageLimit);
        builder.Ignore(propertyExpression: p => p.RemainingUsage);
        #endregion

        #region Concurrency
        builder.ConfigureVersion();
        #endregion
    }
}
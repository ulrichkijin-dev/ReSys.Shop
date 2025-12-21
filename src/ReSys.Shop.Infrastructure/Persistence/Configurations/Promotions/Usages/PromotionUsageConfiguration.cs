using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Domain.Promotions.Usages;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Promotions.Usages;

/// <summary>
/// Configures the database mapping for the <see cref="PromotionUsage"/> entity.
/// </summary>
/// <summary>
/// Configures the database mapping for the <see cref="PromotionUsage"/> entity.
/// </summary>
public sealed class PromotionUsageConfiguration : IEntityTypeConfiguration<PromotionUsage>
{
    /// <summary>
    /// Configures the entity of type <see cref="PromotionUsage"/>.
    /// </summary>
    /// <param name="builder">The builder to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<PromotionUsage> builder)
    {
        #region Table

        builder.ToTable(name: Schema.PromotionUsages);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: x => x.Id);
        #endregion

        #region Indexes

        builder.HasIndex(indexExpression: x => x.PromotionId);

        builder.HasIndex(indexExpression: x => x.Action);

        builder.HasIndex(indexExpression: x => x.CreatedAt);

        builder.HasIndex(indexExpression: x => new { x.PromotionId, x.CreatedAt });
        #endregion

        #region Properties
        builder.Property(propertyExpression: x => x.PromotionId)
            .IsRequired()
            .HasComment(comment: "PromotionId: Identifier of the promotion this audit entry belongs to.");

        builder.Property(propertyExpression: x => x.Action)
            .IsRequired()
            .HasMaxLength(maxLength: 50)
            .HasComment(comment: "Action: Name of the action performed (Created, Updated, Activated, etc.).");

        builder.Property(propertyExpression: x => x.Description)
            .IsRequired()
            .HasMaxLength(maxLength: 500)
            .HasComment(comment: "Description: Detailed explanation of the audit event.");

        builder.Property(propertyExpression: x => x.UserId)
            .HasMaxLength(maxLength: 450)
            .HasComment(comment: "UserId: Identifier of the user who performed the action, if available.");

        builder.Property(propertyExpression: x => x.UserEmail)
            .HasMaxLength(maxLength: 256)
            .HasComment(comment: "UserEmail: Email of the user who performed the action, if available.");

        builder.Property(propertyExpression: x => x.IpAddress)
            .HasMaxLength(maxLength: 45)
            .HasComment(comment: "IpAddress: IP address from which the action originated.");

        builder.Property(propertyExpression: x => x.UserAgent)
            .HasMaxLength(maxLength: 500)
            .HasComment(comment: "UserAgent: Client user-agent string associated with the action.");

        builder.Property(propertyExpression: x => x.ChangesBefore)
            .ConfigureDictionary()
            .HasComment(comment: "ChangesBefore: Dictionary snapshot of entity state before the action.");

        builder.Property(propertyExpression: x => x.ChangesAfter)
            .ConfigureDictionary()
            .HasComment(comment: "ChangesAfter: Dictionary snapshot of entity state after the action.");

        builder.Property(propertyExpression: x => x.Metadata)
            .ConfigureDictionary()
            .HasComment(comment: "Metadata: Additional contextual metadata for the audit entry.");
        #endregion

        #region Relationships

        builder.HasOne(navigationExpression: x => x.Promotion)
            .WithMany(navigationExpression: p => p.PromotionUsages)
            .HasForeignKey(foreignKeyExpression: x => x.PromotionId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
        #endregion
    }
}

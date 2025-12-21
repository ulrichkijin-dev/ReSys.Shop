using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Promotions.Actions;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Promotions.Actions;

/// <summary>
/// Configures the database mapping for the <see cref="PromotionAction"/> entity.
/// </summary>
public sealed class PromotionActionConfiguration : IEntityTypeConfiguration<PromotionAction>
{
    /// <summary>
    /// Configures the entity of type <see cref="PromotionAction"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<PromotionAction> builder)
    {
        #region Table

        builder.ToTable(name: Schema.PromotionActions);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: pu => pu.Id);
        #endregion

        #region Properties

        builder.Property(propertyExpression: pu => pu.Id)
            .ValueGeneratedNever()
            .HasComment(comment: "Id: Unique identifier for the promotion usage action.");

        builder.Property(propertyExpression: pu => pu.PromotionId)
            .IsRequired()
            .HasComment(comment: "PromotionId: Foreign key to the associated Promotion.");

        builder.Property(propertyExpression: pu => pu.Type)
            .HasConversion<string>()
            .HasMaxLength(maxLength: 50)
            .IsRequired()
            .HasComment(comment: "Type: The type of promotion action (e.g., OrderDiscount, ItemDiscount).");

        builder.ConfigureMetadata();
        builder.ConfigureAuditable();

        #endregion

        #region Relationships

        builder.HasOne(navigationExpression: pu => pu.Promotion)
            .WithOne(navigationExpression: p => p.Action)
            .HasForeignKey<PromotionAction>(foreignKeyExpression: pu => pu.PromotionId)
            .IsRequired()
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
        #endregion
    }
}
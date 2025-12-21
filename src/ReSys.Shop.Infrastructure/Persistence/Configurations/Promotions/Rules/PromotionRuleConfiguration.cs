using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Promotions.Rules;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Promotions.Rules;

/// <summary>
/// Configures the database mapping for the <see cref="PromotionRule"/> entity.
/// </summary>
public sealed class PromotionRuleConfiguration : IEntityTypeConfiguration<PromotionRule>
{
    /// <summary>
    /// Configures the entity of type <see cref="PromotionRule"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<PromotionRule> builder)
    {
        #region Table

        builder.ToTable(name: Schema.PromotionRules);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: pr => pr.Id);
        #endregion

        #region Properties

        builder.Property(propertyExpression: pr => pr.Id)
            .ValueGeneratedNever()
            .HasComment(comment: "Id: Unique identifier for the promotion rule. Value generated never.");

        builder.Property(propertyExpression: pr => pr.PromotionId)
            .IsRequired()
            .HasComment(comment: "PromotionId: Foreign key to the associated Promotion.");

        builder.Property(propertyExpression: pr => pr.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasComment(comment: "Type: The type of the promotion rule (e.g., 'UserLoggedIn', 'ProductInCart').");

        builder.Property(propertyExpression: pr => pr.Value)
            .IsRequired()
            .HasMaxLength(maxLength: PromotionRule.Constraints.ValueMaxLength)
            .HasComment(comment: "Value: The value associated with the rule (e.g., a product ID, a minimum quantity).");

        builder.ConfigureAuditable();

        #endregion

        #region Relationships

        builder.HasOne(navigationExpression: pr => pr.Promotion)
            .WithMany(navigationExpression: p => p.PromotionRules)
            .HasForeignKey(foreignKeyExpression: pr => pr.PromotionId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);

        builder.HasMany(navigationExpression: pr => pr.PromotionRuleTaxons)
            .WithOne(navigationExpression: prt => prt.PromotionRule)
            .HasForeignKey(foreignKeyExpression: prt => prt.PromotionRuleId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);

        builder.HasMany(navigationExpression: pr => pr.PromotionRuleUsers)
            .WithOne(navigationExpression: pru => pru.PromotionRule)
            .HasForeignKey(foreignKeyExpression: pru => pru.PromotionRuleId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
        #endregion

        #region Concurrency
        builder.ConfigureVersion();
        #endregion
    }
}

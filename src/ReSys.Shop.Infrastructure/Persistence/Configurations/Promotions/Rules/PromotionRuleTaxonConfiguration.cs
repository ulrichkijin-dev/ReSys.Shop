using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Promotions.Rules;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Promotions.Rules;

/// <summary>
/// Configures the database mapping for the <see cref="PromotionRuleTaxon"/> entity.
/// </summary>
public sealed class PromotionRuleTaxonConfiguration : IEntityTypeConfiguration<PromotionRuleTaxon>
{
    /// <summary>
    /// Configures the entity of type <see cref="PromotionRuleTaxon"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<PromotionRuleTaxon> builder)
    {
        #region Table

        builder.ToTable(name: Schema.PromotionRuleTaxons);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: prt => prt.Id);
        #endregion

        #region Properties

        builder.Property(propertyExpression: prt => prt.Id)
            .ValueGeneratedNever()
            .HasComment(comment: "Id: Unique identifier for the promotion rule taxon. Value generated never.");

        builder.Property(propertyExpression: prt => prt.PromotionRuleId)
            .IsRequired()
            .HasComment(comment: "PromotionRuleId: Foreign key to the associated PromotionRule.");

        builder.Property(propertyExpression: prt => prt.TaxonId)
            .IsRequired()
            .HasComment(comment: "TaxonId: Foreign key to the associated Taxon.");

        builder.ConfigureAuditable();

        #endregion

        #region Relationships

        builder.HasOne(navigationExpression: prt => prt.PromotionRule)
            .WithMany(navigationExpression: pr => pr.PromotionRuleTaxons)
            .HasForeignKey(foreignKeyExpression: prt => prt.PromotionRuleId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);

        builder.HasOne(navigationExpression: prt => prt.Taxon)
            .WithMany(navigationExpression: pr => pr.PromotionRuleTaxons)
            .HasForeignKey(foreignKeyExpression: prt => prt.TaxonId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
        #endregion
    }
}
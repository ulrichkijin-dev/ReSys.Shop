using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Common.Shared;
using ReSys.Shop.Core.Domain.Catalog.Taxonomies.Rules;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Catalog.Taxonomies.Rules;

/// <summary>
/// Configures the database mapping for the <see cref="TaxonRule"/> entity.
/// </summary>
public sealed class TaxonRuleConfiguration : IEntityTypeConfiguration<TaxonRule>
{
    /// <summary>
    /// Configures the entity of type <see cref="TaxonRule"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<TaxonRule> builder)
    {
        #region Table

        builder.ToTable(name: Schema.TaxonRules);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: tr => tr.Id);
        #endregion

        #region Indexes

        builder.HasIndex(indexExpression: tr => new { tr.TaxonId, tr.Type });
        #endregion

        #region Properties

        builder.Property(propertyExpression: tr => tr.Id)
            .HasColumnName(name: "id")
            .ValueGeneratedNever()
            .HasComment(comment: "Id: Unique identifier for the taxon rule. Value generated never.");

        builder.Property(propertyExpression: tr => tr.TaxonId)
            .IsRequired()
            .HasComment(comment: "TaxonId: Foreign key to the associated Taxon.");

        builder.Property(propertyExpression: tr => tr.Type)
            .ConfigureShortText()
            .HasComment(comment: "Type: The type of the rule (e.g., 'product_name', 'product_property').");

        builder.Property(propertyExpression: tr => tr.Value)
            .ConfigureMediumText()
            .HasComment(comment: "Value: The value to match against for the rule.");

        builder.Property(propertyExpression: tr => tr.MatchPolicy)
            .ConfigureShortText()
            .HasComment(comment: "MatchPolicy: The policy for matching (e.g., 'is_equal_to', 'contains').");

        builder.Property(propertyExpression: tr => tr.PropertyName)
            .ConfigureNameOptional(isRequired: false)
            .HasComment(comment: "PropertyName: The name of the product property if the rule type is 'product_property'.");

        builder.ConfigureAuditable();
        #endregion

        #region Relationships

        builder.HasOne(navigationExpression: tr => tr.Taxon)
            .WithMany(navigationExpression: t => t.TaxonRules)
            .HasForeignKey(foreignKeyExpression: tr => tr.TaxonId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
        #endregion
    }
}

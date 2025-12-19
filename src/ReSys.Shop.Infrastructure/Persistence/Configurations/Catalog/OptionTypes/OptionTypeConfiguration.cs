using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Common.Shared;
using ReSys.Shop.Core.Domain.Catalog.OptionTypes;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Catalog.OptionTypes;

/// <summary>
/// Configures the database mapping for the <see cref="OptionType"/> entity.
/// </summary>
public sealed class OptionTypeConfiguration : IEntityTypeConfiguration<OptionType>
{
    /// <summary>
    /// Configures the entity of type <see cref="OptionType"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<OptionType> builder)
    {
        #region Table

        builder.ToTable(name: Schema.OptionTypes);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: ot => ot.Id);
        #endregion

        #region Indexes

        builder.HasIndex(indexExpression: ot => ot.Name).IsUnique();
        #endregion

        #region Properties

        builder.Property(propertyExpression: ot => ot.Id)
            .ValueGeneratedNever()
            .HasComment(comment: "Id: Unique identifier for the option type. Value generated never.");

        builder.Property(propertyExpression: ot => ot.Name)
            .HasMaxLength(maxLength: CommonInput.Constraints.NamesAndUsernames.NameMaxLength)
            .IsRequired()
            .HasComment(comment: "Name: The unique name of the option type (e.g., 'Color', 'Size').");

        builder.Property(propertyExpression: ot => ot.Presentation)
            .HasMaxLength(maxLength: CommonInput.Constraints.NamesAndUsernames.NameMaxLength)
            .IsRequired()
            .HasComment(comment: "Presentation: The human-readable display name of the option type.");

        builder.Property(propertyExpression: ot => ot.Position)
            .IsRequired()
            .HasComment(comment: "Position: The display order of the option type.");

        builder.ConfigureParameterizableName();
        builder.ConfigurePosition();
        builder.ConfigureMetadata();
        builder.ConfigureAuditable();
        #endregion

        #region Relationships

        builder.HasMany(navigationExpression: ot => ot.OptionValues)
            .WithOne(navigationExpression: ov => ov.OptionType)
            .HasForeignKey(foreignKeyExpression: ov => ov.OptionTypeId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);

        builder.HasMany(navigationExpression: ot => ot.ProductOptionTypes)
            .WithOne(navigationExpression: pot => pot.OptionType)
            .HasForeignKey(foreignKeyExpression: pot => pot.OptionTypeId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
        #endregion
    }
}

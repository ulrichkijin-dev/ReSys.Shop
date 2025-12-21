using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Location.Countries;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Location.Countries;

/// <summary>
/// Configures the database mapping for the <see cref="Country"/> entity.
/// </summary>
public sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    /// <summary>
    /// Configures the entity of type <see cref="Country"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        #region Table

        builder.ToTable(name: Schema.Countries);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: c => c.Id);
        #endregion

        #region Properties

        builder.Property(propertyExpression: c => c.Id)
            .ValueGeneratedNever()
            .HasComment(comment: "Id: Unique identifier for the country.");

        builder.Property(propertyExpression: c => c.Name)
            .ConfigureInput(maxLength: CommonInput.Constraints.Text.TinyTextMaxLength)
            .HasComment(comment: "Name: The full name of the country. Required.");

        builder.Property(propertyExpression: c => c.Iso)
            .ConfigureInput(columnName: "iso", maxLength: Country.Constraints.IsoMaxLength)
            .HasComment(comment: "Iso: ISO 3166-1 alpha-2 code.");

        builder.Property(propertyExpression: c => c.Iso3)
            .ConfigureInput(columnName: "iso3", maxLength: Country.Constraints.Iso3MaxLength)
            .HasComment(comment: "Iso3: ISO 3166-1 alpha-3 code.");

        builder.ConfigureAuditable();
        #endregion

        #region Relationships
        builder.HasMany(navigationExpression: c => c.States)
            .WithOne(navigationExpression: s => s.Country)
            .HasForeignKey(foreignKeyExpression: s => s.CountryId)
            .OnDelete(deleteBehavior: DeleteBehavior.SetNull);

        builder.HasMany(navigationExpression: c => c.UserAddresses)
            .WithOne(navigationExpression: ua => ua.Country)
            .HasForeignKey(foreignKeyExpression: ua => ua.CountryId)
            .OnDelete(deleteBehavior: DeleteBehavior.SetNull);

        builder.HasMany(navigationExpression: c => c.StockLocations)
            .WithOne(navigationExpression: sl => sl.Country)
            .HasForeignKey(foreignKeyExpression: sl => sl.CountryId)
            .OnDelete(deleteBehavior: DeleteBehavior.SetNull);

        #endregion
    }
}

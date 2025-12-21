using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Inventories.Locations;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Inventories.Locations;

/// <summary>
/// Configures the database mapping for the <see cref="StockLocation"/> entity.
/// </summary>
public sealed class StockLocationConfiguration : IEntityTypeConfiguration<StockLocation>
{
    /// <summary>
    /// Configures the entity of type <see cref="StockLocation"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<StockLocation> builder)
    {
        #region Table

        builder.ToTable(name: Schema.StockLocations);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: sl => sl.Id);
        #endregion

        #region Indexes

        builder.HasIndex(indexExpression: sl => sl.Name).IsUnique();
        builder.HasIndex(indexExpression: sl => sl.Active);
        builder.HasIndex(indexExpression: sl => sl.Default);
        builder.HasIndex(indexExpression: sl => new { sl.ShipEnabled, sl.IsDeleted })
            .HasDatabaseName(name: "IX_StockLocation_ShipEnabled_IsDeleted");
        builder.HasIndex(indexExpression: sl => new { sl.PickupEnabled, sl.IsDeleted })
            .HasDatabaseName(name: "IX_StockLocation_PickupEnabled_IsDeleted");
        builder.HasIndex(indexExpression: sl => new { sl.Latitude, sl.Longitude })
            .HasDatabaseName(name: "IX_StockLocation_Coordinates");
        #endregion

        #region Properties

        builder.Property(propertyExpression: sl => sl.Id)
            .HasColumnName(name: "id")
            .ValueGeneratedNever()
            .HasComment(comment: "Unique identifier for the stock location.");

        builder.ConfigureParameterizableName();
        builder.ConfigureUniqueName();

        builder.Property(propertyExpression: sl => sl.Name)
            .HasComment(comment: "Name: The internal system name for the stock location (e.g., 'main-warehouse', 'nyc-store').");

        builder.Property(propertyExpression: sl => sl.Presentation)
            .HasComment(comment: "Presentation: The human-readable display name for the stock location (e.g., 'Main Warehouse', 'NYC Retail Store').");

        builder.Property(propertyExpression: sl => sl.Active)
            .IsRequired()
            .HasDefaultValue(value: true)
            .HasComment(comment: "Active: Indicates if the stock location is active. Required, defaults to true.");

        builder.Property(propertyExpression: sl => sl.Default)
            .IsRequired()
            .HasDefaultValue(value: false)
            .HasComment(comment: "Default: Indicates if this is the default stock location. Required, defaults to false.");

        builder.Property(propertyExpression: a => a.Company)
            .HasMaxLength(maxLength: StockLocation.Constraints.CompanyMaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "Company: The company name associated with the stock location address.");

        builder.Property(propertyExpression: a => a.Address1)
            .HasMaxLength(maxLength: StockLocation.Constraints.AddressMaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "Address1: The primary line of the street address.");

        builder.Property(propertyExpression: a => a.Address2)
            .HasMaxLength(maxLength: StockLocation.Constraints.AddressMaxLength)
            .IsRequired(required: false) 
            .HasComment(comment: "Address2: The secondary line of the street address.");

        builder.Property(propertyExpression: a => a.City)
            .HasMaxLength(maxLength: StockLocation.Constraints.CityMaxLength)
            .IsRequired(required: false) 
            .HasComment(comment: "City: The city of the address.");

        builder.Property(propertyExpression: a => a.ZipCode)
            .ConfigurePostalCode(isRequired: false)
            .HasMaxLength(maxLength: StockLocation.Constraints.ZipcodeMaxLength)
            .HasComment(comment: "Zipcode: The postal code of the address.");

        builder.Property(propertyExpression: a => a.Phone)
            .ConfigurePhone(isRequired: false)
            .HasMaxLength(maxLength: StockLocation.Constraints.PhoneMaxLength)
            .HasComment(comment: "Phone: The phone number associated with the address.");

        builder.Property(propertyExpression: a => a.Email)
            .HasMaxLength(maxLength: StockLocation.Constraints.EmailMaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "Email: The email address associated with the stock location.");

        builder.Property(propertyExpression: sl => sl.Type)
            .ConfigurePostgresEnum()
            .HasComment(comment: "Type: Enum indicating location type (Warehouse, RetailStore, Both). Stored as string for flexibility.");

        builder.Property(propertyExpression: sl => sl.ShipEnabled)
            .IsRequired()
            .HasDefaultValue(value: true)
            .HasComment(comment: "ShipEnabled: Whether this location can ship orders. Defaults to true.");

        builder.Property(propertyExpression: sl => sl.PickupEnabled)
            .IsRequired()
            .HasDefaultValue(value: false)
            .HasComment(comment: "PickupEnabled: Whether this location supports store pickup. Defaults to false.");

        builder.Property(propertyExpression: sl => sl.Latitude)
            .HasPrecision(precision: 9, scale: 6)
            .IsRequired(required: false)
            .HasComment(comment: "Latitude: Geographic latitude coordinate (-90 to 90) for distance calculations.");

        builder.Property(propertyExpression: sl => sl.Longitude)
            .HasPrecision(precision: 9, scale: 6)
            .IsRequired(required: false)
            .HasComment(comment: "Longitude: Geographic longitude coordinate (-180 to 180) for distance calculations.");

        builder.Property(propertyExpression: sl => sl.OperatingHours)
            .ConfigureDictionary(isRequired: false)
            .HasColumnType(typeName: "jsonb")
            .HasComment(comment: "OperatingHours: JSON dictionary of operating hours by day of week (e.g., {\"Monday\": \"09:00-17:00\"}).");

        builder.ConfigureMetadata();
        builder.ConfigureAuditable();
        #endregion

        #region Relationships

        builder.HasMany(navigationExpression: sl => sl.StockItems)
            .WithOne(navigationExpression: si => si.StockLocation)
            .HasForeignKey(foreignKeyExpression: si => si.StockLocationId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);

        builder.HasOne(navigationExpression: a => a.Country)
            .WithMany(navigationExpression: m => m.StockLocations)
            .HasForeignKey(foreignKeyExpression: a => a.CountryId)
            .OnDelete(deleteBehavior: DeleteBehavior.SetNull);

        builder.HasOne(navigationExpression: a => a.State)
            .WithMany(navigationExpression: m => m.StockLocations)
            .HasForeignKey(foreignKeyExpression: a => a.StateId)
            .OnDelete(deleteBehavior: DeleteBehavior.SetNull);
        #endregion

        #region Concurrency
        builder.ConfigureVersion();
        #endregion
    }
}

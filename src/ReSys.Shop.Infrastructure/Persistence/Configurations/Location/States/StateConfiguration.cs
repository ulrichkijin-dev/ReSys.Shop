using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Common.Shared;
using ReSys.Shop.Core.Domain.Location.States;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Location.States;

/// <summary>
/// Configures the database mapping for the <see cref="State"/> entity.
/// </summary>
public sealed class StateConfiguration : IEntityTypeConfiguration<State>
{
    /// <summary>
    /// Configures the entity of type <see cref="State"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<State> builder)
    {
        #region Table

        builder.ToTable(name: Schema.States);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: s => s.Id);
        #endregion

        #region Properties

        builder.Property(propertyExpression: s => s.Id)
            .ValueGeneratedNever()
            .HasComment(comment: "Id: Unique identifier for the state.");

        builder.Property(propertyExpression: s => s.Name)
            .HasMaxLength(maxLength: CommonInput.Constraints.NamesAndUsernames.NameMaxLength)
            .IsRequired()
            .HasComment(comment: "Name: The full name of the state. Required.");

        builder.Property(propertyExpression: s => s.Abbr)
            .HasMaxLength(maxLength: CommonInput.Constraints.DateAndTime.TimeMaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "Abbr: The abbreviation for the state (e.g., 'CA'). Optional.");

        builder.ConfigureAuditable();
        #endregion

        #region Relationships
        builder.Property(propertyExpression: s => s.CountryId)
            .IsRequired()
            .HasComment(comment: "CountryId: Foreign key to the Country entity.");

        builder.HasOne(navigationExpression: s => s.Country)
            .WithMany(navigationExpression: c => c.States)
            .HasForeignKey(foreignKeyExpression: s => s.CountryId)
            .OnDelete(deleteBehavior: DeleteBehavior.SetNull);

        builder.HasMany(navigationExpression: s => s.UserAddresses)
            .WithOne(navigationExpression: ua => ua.State)
            .HasForeignKey(foreignKeyExpression: ua => ua.StateId)
            .IsRequired(required: false)
            .OnDelete(deleteBehavior: DeleteBehavior.SetNull);

        builder.HasMany(navigationExpression: s => s.StockLocations)
            .WithOne(navigationExpression: sl => sl.State)
            .HasForeignKey(foreignKeyExpression: sl => sl.StateId)
            .IsRequired(required: false)
            .OnDelete(deleteBehavior: DeleteBehavior.SetNull);

        #endregion
    }
}

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Identity.Roles;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Identity.Roles;

/// <summary>
/// Configures the database mapping for the <see cref="Role"/> entity.
/// </summary>
public sealed class RoleConfigurations : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable(name: Schema.Roles);

        builder.HasKey(keyExpression: r => r.Id);

        #region Properties

        builder.Property(propertyExpression: e => e.Name)
            .IsRequired()
            .HasMaxLength(maxLength: CommonInput.Constraints.NamesAndUsernames.NameMaxLength);

        builder.Property(propertyExpression: e => e.NormalizedName)
            .IsRequired()
            .HasMaxLength(maxLength: CommonInput.Constraints.NamesAndUsernames.NameMaxLength);

        builder.Property(propertyExpression: e => e.Description)
            .HasMaxLength(maxLength: CommonInput.Constraints.Text.MediumTextMaxLength);

        builder.Property(propertyExpression: e => e.DisplayName)
            .HasMaxLength(maxLength: CommonInput.Constraints.NamesAndUsernames.NameMaxLength)
            .IsRequired(required: false);

        builder.Property(propertyExpression: e => e.IsDefault)
            .IsRequired();

        builder.Property(propertyExpression: e => e.Priority)
            .IsRequired();

        builder.Property(propertyExpression: e => e.IsSystemRole)
            .IsRequired();

        builder.ConfigureAuditable();
        builder.ConfigureVersion();
        #endregion

        #region Indexes

        builder.HasIndex(indexExpression: r => r.NormalizedName)
            .IsUnique();

        builder.HasIndex(indexExpression: r => r.IsSystemRole);

        builder.HasIndex(indexExpression: r => r.IsDefault);

        builder.HasIndex(indexExpression: r => r.Priority);

        #endregion

        #region Relationships

        builder.HasMany(navigationExpression: e => e.UserRoles)
            .WithOne(navigationExpression: e => e.Role)
            .HasForeignKey(foreignKeyExpression: ur => ur.RoleId)
            .IsRequired();

        builder.HasMany(navigationExpression: e => e.RoleClaims)
            .WithOne(navigationExpression: e => e.Role)
            .HasForeignKey(foreignKeyExpression: rc => rc.RoleId)
            .IsRequired();
        #endregion

        #region Concurrency
        builder.ConfigureVersion();
        #endregion
    }
}

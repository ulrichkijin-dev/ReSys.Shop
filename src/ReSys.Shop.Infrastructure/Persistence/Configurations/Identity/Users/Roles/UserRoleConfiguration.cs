using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Identity.Users.Roles;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Identity.Users.Roles;

/// <summary>
/// Configures the database mapping for the <see cref="UserRole"/> join entity.
/// </summary>
public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    /// <summary>
    /// Configures the entity of type <see cref="UserRole"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        #region Table

        builder.ToTable(name: Schema.UserRoles);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: ur => new { ur.UserId, ur.RoleId });
        #endregion

        #region Properties
        builder.Property(propertyExpression: ur => ur.UserId)
            .HasComment(comment: "UserId: Foreign key to the associated ApplicationUser.");

        builder.Property(propertyExpression: ur => ur.RoleId)
            .HasComment(comment: "RoleId: Foreign key to the associated ApplicationRole.");

        builder.ConfigureAssignable();

        #endregion

        #region Relationships

        builder.HasOne(navigationExpression: ur => ur.User)
            .WithMany(navigationExpression: u => u.UserRoles)
            .HasForeignKey(foreignKeyExpression: ur => ur.UserId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);

        builder.HasOne(navigationExpression: ur => ur.Role)
            .WithMany(navigationExpression: r => r.UserRoles)
            .HasForeignKey(foreignKeyExpression: ur => ur.RoleId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);
        #endregion
    }
}
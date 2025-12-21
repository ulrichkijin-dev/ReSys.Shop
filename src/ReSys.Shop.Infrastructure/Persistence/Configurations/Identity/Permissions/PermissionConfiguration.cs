using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Identity.Permissions;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Identity.Permissions;

/// <summary>
/// Configures the database mapping for the <see cref="AccessPermission"/> entity.
/// </summary>
public sealed class PermissionConfiguration : IEntityTypeConfiguration<AccessPermission>
{
    /// <summary>
    /// Configures the entity of type <see cref="AccessPermission"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<AccessPermission> builder)
    {
        #region Table

        builder.ToTable(name: Schema.AccessPermissions);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: p => p.Id);
        #endregion

        #region Properties

        builder.Property(propertyExpression: p => p.Id)
            .ValueGeneratedNever()
            .HasComment(comment: "Id: Unique identifier for the access permission. Value generated never.");

        builder.Property(propertyExpression: p => p.Name)
            .IsRequired()
            .HasMaxLength(maxLength: AccessPermission.Constraints.MaxNameLength)
            .HasComment(comment: "Name: The unique name of the permission (e.g., 'admin.users.create').");

        builder.Property(propertyExpression: p => p.Area)
            .IsRequired()
            .HasMaxLength(maxLength: AccessPermission.Constraints.MaxSegmentLength)
            .HasComment(comment: "Area: The area or domain of the permission (e.g., 'admin').");

        builder.Property(propertyExpression: p => p.Resource)
            .IsRequired()
            .HasMaxLength(maxLength: AccessPermission.Constraints.MaxSegmentLength)
            .HasComment(comment: "Resource: The resource the permission applies to (e.g., 'users').");

        builder.Property(propertyExpression: p => p.Action)
            .IsRequired()
            .HasMaxLength(maxLength: AccessPermission.Constraints.MaxSegmentLength)
            .HasComment(comment: "Action: The action allowed by the permission (e.g., 'create').");

        builder.Property(propertyExpression: p => p.DisplayName)
            .HasMaxLength(maxLength: AccessPermission.Constraints.MaxDisplayNameLength)
            .IsRequired(required: false)
            .HasComment(comment: "DisplayName: A user-friendly display name for the permission.");

        builder.Property(propertyExpression: p => p.Description)
            .HasMaxLength(maxLength: AccessPermission.Constraints.MaxDescriptionLength)
            .IsRequired(required: false)
            .HasComment(comment: "Description: A detailed description of what the permission allows.");

        builder.Property(propertyExpression: p => p.Category)
            .ConfigurePostgresEnumOptional()
            .HasComment(comment: "Category: The category of the permission.");

        builder.ConfigureAuditable();
        #endregion

        #region Indexes

        builder.HasIndex(indexExpression: p => p.Name)
            .IsUnique();

        builder.HasIndex(indexExpression: p => new { p.Area, p.Resource, p.Action })
            .IsUnique();
        #endregion
    }
}
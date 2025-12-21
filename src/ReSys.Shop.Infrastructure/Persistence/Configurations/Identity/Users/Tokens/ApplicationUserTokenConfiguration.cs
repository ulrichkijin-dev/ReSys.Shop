using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Domain.Identity.Users.Tokens;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Identity.Users.Tokens;
/// <summary>
/// Configures the database mapping for the <see cref="UserToken"/> entity.
/// </summary>
public sealed class ApplicationUserTokenConfiguration : IEntityTypeConfiguration<UserToken>
{
    /// <summary>
    /// Configures the entity of type <see cref="UserToken"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
        #region Table

        builder.ToTable(name: Schema.UserTokens);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: ut => new { ut.UserId, ut.LoginProvider, ut.Name });
        #endregion

        #region Properties
        builder.Property(propertyExpression: ut => ut.UserId)
            .HasComment(comment: "UserId: Foreign key to the associated ApplicationUser.");
        builder.Property(propertyExpression: ut => ut.LoginProvider)
            .HasComment(comment: "LoginProvider: The login provider for the user (e.g., 'Google', 'Facebook').");
        builder.Property(propertyExpression: ut => ut.Name)
            .HasComment(comment: "Name: The name of the user token.");
        builder.Property(propertyExpression: ut => ut.Value)
            .HasComment(comment: "Value: The value of the user token.");
        #endregion
    }
}

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Domain.Identity.Users.Logins;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Identity.Users.Logins;

/// <summary>
/// Configures the database mapping for the <see cref="UserLogin"/> entity.
/// </summary>
public sealed class UserLoginConfiguration : IEntityTypeConfiguration<UserLogin>
{
    /// <summary>
    /// Configures the entity of type <see cref="UserLogin"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<UserLogin> builder)
    {
        #region Table

        builder.ToTable(name: Schema.UserLogins);
        #endregion

        #region Primary Key

        #endregion

        #region Properties

        #endregion
    }
}
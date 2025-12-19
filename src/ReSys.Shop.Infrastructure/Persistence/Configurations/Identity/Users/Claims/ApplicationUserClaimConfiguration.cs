using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Identity.Users.Claims;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Identity.Users.Claims;
/// <summary>
/// Configures the database mapping for the <see cref="UserClaim"/> entity.
/// </summary>
public class ApplicationUserClaimConfiguration : IEntityTypeConfiguration<UserClaim>
{
    /// <summary>
    /// Configures the entity of type <see cref="UserClaim"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<UserClaim> builder)
    {
        #region Table

        builder.ToTable(name: Schema.UserClaims);
        #endregion

        #region Primary Key

        #endregion

        #region Properties

        #endregion

        builder.ConfigureAssignable();
    }
}
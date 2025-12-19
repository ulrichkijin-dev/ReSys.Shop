using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Identity.Roles.Claims;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Identity.Roles.Claims;
/// <summary>
/// Configures the database mapping for the <see cref="RoleClaim"/> entity.
/// </summary>
public sealed class RoleClaimConfiguration : IEntityTypeConfiguration<RoleClaim>
{
    /// <summary>
    /// Configures the entity of type <see cref="RoleClaim"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<RoleClaim> builder)
    {
        #region Table

        builder.ToTable(name: Schema.RoleClaims);
        
        #endregion

        #region Primary Key

        #endregion

        #region Properties

        #endregion

        builder.ConfigureAssignable();
    }
}

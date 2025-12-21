using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Identity.Tokens;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Identity.Tokens;

/// <summary>
/// Configures the database mapping for the <see cref="RefreshToken"/> entity.
/// </summary>
public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    /// <summary>
    /// Configures the entity of type <see cref="RefreshToken"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        #region Table

        builder.ToTable(name: Schema.RefreshTokens);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: e => e.Id);
        #endregion

        #region Indexes

        builder.HasIndex(indexExpression: e => e.TokenHash).IsUnique();
        builder.HasIndex(indexExpression: e => e.UserId);
        #endregion

        #region Properties

        builder.Property(propertyExpression: e => e.Id)
            .ValueGeneratedNever()
            .HasComment(comment: "Id: Unique identifier for the refresh token. Value generated never.");

        builder.Property(propertyExpression: e => e.TokenHash)
            .IsRequired()
            .HasMaxLength(maxLength: CommonInput.Constraints.Text.ShortTextMaxLength)
            .HasComment(comment: "TokenHash: The hashed version of the refresh token.");

        builder.ConfigureAuditable();
        builder.ConfigureAssignable();

        builder.Property(propertyExpression: e => e.CreatedByIp)
            .IsRequired()
            .HasMaxLength(maxLength: CommonInput.Constraints.Network.IpV4MaxLength)
            .HasComment(comment: "CreatedByIp: The IP address from which the token was created.");

        builder.Property(propertyExpression: e => e.RevokedByIp)
            .HasMaxLength(maxLength: CommonInput.Constraints.Network.IpV4MaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "RevokedByIp: The IP address from which the token was revoked, if applicable.");

        builder.Property(propertyExpression: e => e.ExpiresAt)
            .IsRequired()
            .HasComment(comment: "ExpiresAt: The expiration date and time of the refresh token.");

        builder.Property(propertyExpression: e => e.RevokedAt)
            .IsRequired(required: false)
            .HasComment(comment: "RevokedAt: The date and time when the refresh token was revoked.");
        #endregion

        #region Relationships

        builder.HasOne(navigationExpression: e => e.User)
            .WithMany(navigationExpression: u => u.RefreshTokens)
            .HasForeignKey(foreignKeyExpression: e => e.UserId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);

        #endregion
    }
}

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Identity.Users;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Identity.Users;

/// <summary>
/// Configures the database mapping for the <see cref="User"/> entity.
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>
    /// Configures the entity of type <see cref="User"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(name: Schema.Users);

        builder.HasKey(keyExpression: u => u.Id);

        builder.HasIndex(indexExpression: e => e.NormalizedEmail).IsUnique();
        builder.HasIndex(indexExpression: e => e.NormalizedUserName).IsUnique();
        builder.HasIndex(indexExpression: e => e.PhoneNumber);

        #region Base Identity Properties

        builder.Property(propertyExpression: u => u.UserName)
            .HasMaxLength(maxLength: CommonInput.Constraints.NamesAndUsernames.UsernameMaxLength)
            .IsRequired()
            .HasComment(comment: "UserName: The user's chosen username.");

        builder.Property(propertyExpression: u => u.NormalizedUserName)
            .HasMaxLength(maxLength: CommonInput.Constraints.NamesAndUsernames.UsernameMaxLength)
            .IsRequired()
            .HasComment(comment: "NormalizedUserName: The normalized username for efficient lookups.");

        builder.Property(propertyExpression: u => u.Email)
            .HasMaxLength(maxLength: CommonInput.Constraints.Email.MaxLength)
            .IsRequired()
            .HasComment(comment: "Email: The user's email address.");

        builder.Property(propertyExpression: u => u.NormalizedEmail)
            .HasMaxLength(maxLength: CommonInput.Constraints.Email.MaxLength)
            .IsRequired()
            .HasComment(comment: "NormalizedEmail: The normalized email for efficient lookups.");

        builder.Property(propertyExpression: u => u.PhoneNumber)
            .HasMaxLength(maxLength: CommonInput.Constraints.PhoneNumbers.E164MaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "PhoneNumber: The user's phone number.");

        #endregion

        #region Custom Profile Properties

        builder.Property(propertyExpression: u => u.FirstName)
            .HasMaxLength(maxLength: CommonInput.Constraints.NamesAndUsernames.NameMaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "FirstName: The user's first name.");

        builder.Property(propertyExpression: u => u.LastName)
            .HasMaxLength(maxLength: CommonInput.Constraints.NamesAndUsernames.NameMaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "LastName: The user's last name.");

        builder.Property(propertyExpression: u => u.DateOfBirth)
            .IsRequired(required: false)
            .HasComment(comment: "DateOfBirth: The user's date of birth.");

        builder.Property(propertyExpression: u => u.ProfileImagePath)
            .HasMaxLength(maxLength: CommonInput.Constraints.UrlAndUri.UrlMaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "ProfileImagePath: The path to the user's profile image.");

        #endregion

        #region Sign-In Tracking

        builder.Property(propertyExpression: u => u.LastSignInAt)
            .IsRequired(required: false)
            .HasComment(comment: "LastSignInAt: The timestamp of the user's last sign-in.");

        builder.Property(propertyExpression: u => u.LastSignInIp)
            .HasMaxLength(maxLength: CommonInput.Constraints.Network.IpV4MaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "LastSignInIp: The IP address from which the user last signed in.");

        builder.Property(propertyExpression: u => u.CurrentSignInAt)
            .IsRequired(required: false)
            .HasComment(comment: "CurrentSignInAt: The timestamp of the user's current sign-in.");

        builder.Property(propertyExpression: u => u.CurrentSignInIp)
            .HasMaxLength(maxLength: CommonInput.Constraints.Network.IpV4MaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "CurrentSignInIp: The IP address from which the user is currently signed in.");

        builder.Property(propertyExpression: u => u.SignInCount)
            .IsRequired()
            .HasDefaultValue(value: 0)
            .HasComment(comment: "SignInCount: The total number of times the user has signed in.");

        #endregion

        #region Auditable Properties
        builder.ConfigureAuditable();
        #endregion

        #region Concurrency
        builder.ConfigureVersion();
        #endregion

        #region Ignored Properties

        builder.Ignore(propertyExpression: u => u.FullName);
        builder.Ignore(propertyExpression: u => u.HasProfile);
        builder.Ignore(propertyExpression: u => u.IsActive);
        builder.Ignore(propertyExpression: u => u.DefaultBillingAddress);
        builder.Ignore(propertyExpression: u => u.DefaultShippingAddress);
        builder.Ignore(propertyExpression: u => u.DomainEvents);

        #endregion

        #region Relationships

        builder.HasMany(navigationExpression: e => e.Claims)
            .WithOne(navigationExpression: e => e.User)
            .HasForeignKey(foreignKeyExpression: uc => uc.UserId)
            .IsRequired();

        builder.HasMany(navigationExpression: e => e.UserLogins)
            .WithOne(navigationExpression: e => e.User)
            .HasForeignKey(foreignKeyExpression: ul => ul.UserId)
            .IsRequired();

        builder.HasMany(navigationExpression: e => e.UserTokens)
            .WithOne(navigationExpression: e => e.User)
            .HasForeignKey(foreignKeyExpression: ut => ut.UserId)
            .IsRequired();

        builder.HasMany(navigationExpression: e => e.UserRoles)
            .WithOne(navigationExpression: e => e.User)
            .HasForeignKey(foreignKeyExpression: ur => ur.UserId)
            .IsRequired();

        builder.HasMany(navigationExpression: u => u.RefreshTokens)
            .WithOne(navigationExpression: rt => rt.User)
            .HasForeignKey(foreignKeyExpression: rt => rt.UserId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);

        builder.HasMany(navigationExpression: u => u.UserAddresses)
            .WithOne(navigationExpression: ua => ua.User)
            .HasForeignKey(foreignKeyExpression: ua => ua.UserId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);

        #endregion
    }
}

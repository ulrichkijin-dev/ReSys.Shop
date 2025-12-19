using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Common.Shared;
using ReSys.Shop.Core.Domain.Settings.PaymentMethods;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Settings.PaymentMethods;

/// <summary>
/// Configures the database mapping for the <see cref="PaymentMethod"/> entity.
/// </summary>
public sealed class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    /// <summary>
    /// Configures the entity of type <see cref="PaymentMethod"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        #region Table

        builder.ToTable(name: Schema.PaymentMethods);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: pm => pm.Id);
        #endregion

        #region Indexes

        builder.HasIndex(indexExpression: pm => pm.Name).IsUnique();
        #endregion

        #region Properties

        builder.Property(propertyExpression: pm => pm.Id)
            .ValueGeneratedNever()
            .HasComment(comment: "Id: Unique identifier for the payment method. Value generated never.");

        builder.Property(propertyExpression: pm => pm.Name)
            .IsRequired()
            .HasMaxLength(maxLength: PaymentMethod.Constraints.NameMaxLength)
            .HasComment(comment: "Name: The name of the payment method.");

        builder.Property(propertyExpression: pm => pm.Description)
            .HasMaxLength(maxLength: CommonInput.Constraints.Text.DescriptionMaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "Description: A detailed description of the payment method.");

        builder.Property(propertyExpression: pm => pm.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasComment(comment: "Type: The type of payment method (e.g., 'CreditCard', 'PayPal').");

        builder.Property(propertyExpression: pm => pm.Active)
            .IsRequired()
            .HasComment(comment: "Active: Indicates if the payment method is active.");

        builder.Property(propertyExpression: pm => pm.AutoCapture)
            .IsRequired()
            .HasComment(comment: "AutoCapture: Indicates if payments made with this method should be automatically captured.");

        builder.Property(propertyExpression: pm => pm.DisplayOn)
            .IsRequired()
            .HasConversion<string>()
            .HasComment(comment: "DisplayOn: Specifies where the payment method should be displayed (e.g., 'Frontend', 'Backend').");

        builder.Property(propertyExpression: pm => pm.DeletedAt)
            .IsRequired(required: false)
            .HasComment(comment: "DeletedAt: The timestamp when the payment method was soft deleted (null if not deleted).");

        builder.ConfigureParameterizableName();
        builder.ConfigurePosition();
        builder.ConfigureMetadata();
        builder.ConfigureAuditable();
        #endregion

        #region Relationships
        builder.HasMany(navigationExpression: pm => pm.Payments)
            .WithOne(navigationExpression: p => p.PaymentMethod)
            .HasForeignKey(foreignKeyExpression: p => p.PaymentMethodId)
            .OnDelete(deleteBehavior: DeleteBehavior.Restrict);

        #endregion

        #region Concurrency
        builder.ConfigureVersion();
        #endregion
    }
}

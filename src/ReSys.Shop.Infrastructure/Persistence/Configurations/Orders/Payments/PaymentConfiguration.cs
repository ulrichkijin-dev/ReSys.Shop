using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Common.Shared;
using ReSys.Shop.Core.Domain.Orders.Payments;

namespace ReSys.Shop.Infrastructure.Persistence.Configurations.Orders.Payments;

/// <summary>
/// Configures the database mapping for the <see cref="Payment"/> entity.
/// </summary>
public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    /// <summary>
    /// Configures the entity of type <see cref="Payment"/>.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        #region Table

        builder.ToTable(name: Schema.Payments);
        #endregion

        #region Primary Key

        builder.HasKey(keyExpression: p => p.Id);
        #endregion

        #region Properties

        builder.Property(propertyExpression: p => p.Id)
            .HasColumnName(name: "id")
            .ValueGeneratedNever()
            .HasComment(comment: "Id: Unique identifier for the payment. Value generated never.");

        builder.Property(propertyExpression: p => p.OrderId)
            .IsRequired()
            .HasComment(comment: "OrderId: Foreign key to the associated Order.");

        builder.Property(propertyExpression: p => p.AmountCents)
            .IsRequired()
            .HasComment(comment: "AmountCents: The payment amount in cents.");

        builder.Property(propertyExpression: p => p.Currency)
            .HasMaxLength(maxLength: CommonInput.Constraints.Text.TinyTextMaxLength)
            .IsRequired()
            .HasDefaultValue(value: "USD")
            .HasComment(comment: "Currency: The currency of the payment.");

        builder.Property(propertyExpression: p => p.State)
            .HasConversion<string>()
            .HasMaxLength(maxLength: 50)
            .IsRequired()
            .HasComment(comment: "State: Current state of the payment (e.g., Pending, Completed).");

        builder.Property(propertyExpression: p => p.ReferenceTransactionId)
            .HasMaxLength(maxLength: Payment.Constraints.ReferenceTransactionIdMaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "ReferenceTransactionId: The transaction ID from the payment gateway.");

        builder.Property(propertyExpression: p => p.GatewayAuthCode)
            .HasMaxLength(maxLength: Payment.Constraints.GatewayAuthCodeMaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "GatewayAuthCode: Authorization code from payment gateway.");

        builder.Property(propertyExpression: p => p.GatewayErrorCode)
            .HasMaxLength(maxLength: Payment.Constraints.GatewayErrorCodeMaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "GatewayErrorCode: Error code from payment gateway.");

        builder.Property(propertyExpression: p => p.AuthorizedAt)
            .IsRequired(required: false)
            .HasComment(comment: "AuthorizedAt: Timestamp when the payment was authorized.");

        builder.Property(propertyExpression: p => p.CapturedAt)
            .IsRequired(required: false)
            .HasComment(comment: "CapturedAt: Timestamp when the payment was captured.");

        builder.Property(propertyExpression: p => p.VoidedAt)
            .IsRequired(required: false)
            .HasComment(comment: "VoidedAt: Timestamp when the payment was voided.");

        builder.Property(propertyExpression: p => p.PaymentMethodType)
            .HasMaxLength(maxLength: Payment.Constraints.PaymentMethodTypeMaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "PaymentMethodType: The type of payment method used.");

        builder.Property(propertyExpression: p => p.FailureReason)
            .HasMaxLength(maxLength: Payment.Constraints.FailureReasonMaxLength)
            .IsRequired(required: false)
            .HasComment(comment: "FailureReason: Reason for payment failure.");

        builder.Property(propertyExpression: p => p.PaymentMethodId)
            .IsRequired(required: false)
            .HasComment(comment: "PaymentMethodId: Foreign key to the associated PaymentMethod.");

        builder.Property(propertyExpression: p => p.IdempotencyKey)
            .HasMaxLength(maxLength: 255)
            .IsRequired(required: false)
            .HasComment(comment: "IdempotencyKey: Key for idempotent payment processing.");

        builder.Property(propertyExpression: e => e.RowVersion)
            .IsRowVersion()
            .HasComment(comment: "RowVersion: Used for optimistic concurrency control.");

        builder.ConfigureAuditable();

        #endregion

        #region Relationships

        builder.HasOne(navigationExpression: p => p.Order)
            .WithMany(navigationExpression: o => o.Payments)
            .HasForeignKey(foreignKeyExpression: p => p.OrderId)
            .OnDelete(deleteBehavior: DeleteBehavior.Cascade);

        builder.HasOne(navigationExpression: p => p.PaymentMethod)
            .WithMany(navigationExpression: pm => pm.Payments)
            .HasForeignKey(foreignKeyExpression: p => p.PaymentMethodId)
            .IsRequired(required: false)
            .OnDelete(deleteBehavior: DeleteBehavior.SetNull);

        #endregion

        #region Indexes

        builder.HasIndex(indexExpression: p => p.OrderId);
        builder.HasIndex(indexExpression: p => p.PaymentMethodId);
        builder.HasIndex(indexExpression: p => p.State);
        builder.HasIndex(indexExpression: p => p.ReferenceTransactionId).IsUnique(unique: false);
        builder.HasIndex(indexExpression: p => p.IdempotencyKey).IsUnique(unique: false);
        #endregion

        #region Ignored Properties

        builder.Ignore(propertyExpression: p => p.IsPending);
        builder.Ignore(propertyExpression: p => p.IsAuthorizing);
        builder.Ignore(propertyExpression: p => p.IsAuthorized);
        builder.Ignore(propertyExpression: p => p.IsCapturing);
        builder.Ignore(propertyExpression: p => p.IsCompleted);
        builder.Ignore(propertyExpression: p => p.IsVoid);
        builder.Ignore(propertyExpression: p => p.IsFailed);
        builder.Ignore(propertyExpression: p => p.Amount);

        #endregion
    }
}
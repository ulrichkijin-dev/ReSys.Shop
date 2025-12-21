using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Common.Domain.Events;
using ReSys.Shop.Core.Domain.Settings.PaymentMethods;

namespace ReSys.Shop.Core.Domain.Orders.Payments;

public sealed class Payment : Aggregate, IHasMetadata
{
    public enum PaymentState
    {
        Pending = 0,
        Authorizing = 1,
        Authorized = 2,
        Capturing = 3,
        Completed = 4,
        Refunded = 5,
        Failed = 7,
        Void = 8,
        RequiresAction = 9
    }

    #region Constraints
    public static class Constraints
    {
        public const long AmountCentsMinValue = 0;
        public const int CurrencyMaxLength = CommonInput.Constraints.CurrencyAndLanguage.CurrencyCodeLength;
        public const int PaymentMethodTypeMaxLength = CommonInput.Constraints.NamesAndUsernames.NameMaxLength;
        public const int ReferenceTransactionIdMaxLength = 100;
        public const int GatewayAuthCodeMaxLength = 50;
        public const int GatewayErrorCodeMaxLength = 100;
        public const int FailureReasonMaxLength = CommonInput.Constraints.Text.LongTextMaxLength;
    }
    #endregion

    #region Errors
    public static class Errors
    {
        public static Error AlreadyCaptured => Error.Validation(code: "Payment.AlreadyCaptured", description: "Payment already captured.");
        public static Error CannotVoidCaptured => Error.Validation(code: "Payment.CannotVoidCaptured", description: "Cannot void captured or completed payment.");
        public static Error NotFound(Guid id) => Error.NotFound(code: "Payment.NotFound", description: $"Payment with ID '{id}' was not found.");
        public static Error InvalidAmountCents => Error.Validation(code: "Payment.InvalidAmountCents", description: $"Amount cents must be at least {Constraints.AmountCentsMinValue}.");
        public static Error CurrencyRequired => CommonInput.Errors.Required(prefix: nameof(Payment), field: nameof(Currency));
        public static Error CurrencyTooLong => CommonInput.Errors.TooLong(prefix: nameof(Payment), field: nameof(Currency), maxLength: Constraints.CurrencyMaxLength);
        public static Error PaymentMethodTypeRequired => CommonInput.Errors.Required(prefix: nameof(Payment), field: nameof(PaymentMethodType));
        public static Error PaymentMethodTypeTooLong => CommonInput.Errors.TooLong(prefix: nameof(Payment), field: nameof(PaymentMethodType), maxLength: Constraints.PaymentMethodTypeMaxLength);
        public static Error ReferenceTransactionIdRequired => CommonInput.Errors.Required(prefix: nameof(Payment), field: nameof(ReferenceTransactionId));
        public static Error ReferenceTransactionIdTooLong => CommonInput.Errors.TooLong(prefix: nameof(Payment), field: nameof(ReferenceTransactionId), maxLength: Constraints.ReferenceTransactionIdMaxLength);
        public static Error GatewayAuthCodeTooLong => CommonInput.Errors.TooLong(prefix: nameof(Payment), field: nameof(GatewayAuthCode), maxLength: Constraints.GatewayAuthCodeMaxLength);
        public static Error GatewayErrorCodeTooLong => CommonInput.Errors.TooLong(prefix: nameof(Payment), field: nameof(GatewayErrorCode), maxLength: Constraints.GatewayErrorCodeMaxLength);
        public static Error FailureReasonTooLong => CommonInput.Errors.TooLong(prefix: nameof(Payment), field: nameof(FailureReason), maxLength: Constraints.FailureReasonMaxLength);
        public static Error IdempotencyKeyConflict => Error.Conflict(code: "Payment.IdempotencyKeyConflict", description: "Payment operation with this idempotency key already exists with a different state or parameters.");
        public static Error InvalidStateTransition(PaymentState from, PaymentState to) => Error.Validation(code: "Payment.InvalidStateTransition", description: $"Cannot transition from {from} to {to}.");
        public static Error AuthorizationRequired => Error.Validation(code: "Payment.AuthorizationRequired", description: "Payment must be authorized before capture.");
    }
    #endregion

    #region Properties
    public Guid OrderId { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public decimal AmountCents { get; set; }
    public string Currency { get; set; } = string.Empty;
    public PaymentState State { get; set; } = PaymentState.Pending;
    public string PaymentMethodType { get; set; } = string.Empty;

    public string? ReferenceTransactionId { get; set; }
    public string? GatewayAuthCode { get; set; }
    public string? GatewayErrorCode { get; set; }

    public DateTimeOffset? AuthorizedAt { get; set; }
    public DateTimeOffset? CapturedAt { get; set; }
    public DateTimeOffset? VoidedAt { get; set; }
    public string? FailureReason { get; set; }
    public string? IdempotencyKey { get; set; }

    public IDictionary<string, object?>? PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?>? PrivateMetadata { get; set; } = new Dictionary<string, object?>();
    #endregion

    #region Relationships
    public Order Order { get; set; } = null!;
    public PaymentMethod? PaymentMethod { get; set; }
    #endregion
    #region Computed Properties
    public bool IsPending => State == PaymentState.Pending;
    public bool IsAuthorizing => State == PaymentState.Authorizing;
    public bool IsAuthorized => State == PaymentState.Authorized;
    public bool IsCapturing => State == PaymentState.Capturing;
    public bool IsCompleted => State == PaymentState.Completed;
    public bool IsVoid => State == PaymentState.Void;
    public bool IsFailed => State == PaymentState.Failed;
    public bool IsRefunded => State == PaymentState.Refunded;
    public bool IsRequiresAction => State == PaymentState.RequiresAction;
    public decimal Amount => AmountCents / 100m;
    #endregion
    #region Constructors
    private Payment() { }
    #endregion

    #region Factory Methods
    public static ErrorOr<Payment> Create(Guid orderId, decimal amountCents, string currency, string paymentMethodType, Guid paymentMethodId, string? idempotencyKey = null)
    {
        if (amountCents < Constraints.AmountCentsMinValue) return Errors.InvalidAmountCents;
        if (string.IsNullOrWhiteSpace(value: currency)) return Errors.CurrencyRequired;
        if (currency.Length > Constraints.CurrencyMaxLength) return Errors.CurrencyTooLong;
        if (string.IsNullOrWhiteSpace(value: paymentMethodType)) return Errors.PaymentMethodTypeRequired;
        if (paymentMethodType.Length > Constraints.PaymentMethodTypeMaxLength) return Errors.PaymentMethodTypeTooLong;

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            PaymentMethodId = paymentMethodId,
            AmountCents = amountCents,
            Currency = currency,
            State = PaymentState.Pending,
            PaymentMethodType = paymentMethodType,
            IdempotencyKey = idempotencyKey,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        payment.AddDomainEvent(domainEvent: new Events.PaymentCreated(PaymentId: payment.Id, OrderId: orderId, IdempotencyKey: idempotencyKey));

        return payment;
    }
    #endregion

    #region Business Logic - Authorize

    /// <summary>
    /// Marks the payment as authorized, setting the transaction ID and state.
    /// </summary>
    public ErrorOr<Payment> MarkAsAuthorized(string transactionId, string? gatewayAuthCode = null)
    {
        if (State == PaymentState.Authorized) return this;
        
        if (State != PaymentState.Pending && State != PaymentState.Authorizing && State != PaymentState.RequiresAction)
            return Errors.InvalidStateTransition(from: State, to: PaymentState.Authorized);

        ReferenceTransactionId = transactionId;
        GatewayAuthCode = gatewayAuthCode;
        State = PaymentState.Authorized;
        AuthorizedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(domainEvent: new Events.PaymentAuthorized(PaymentId: Id, OrderId: OrderId, ReferenceTransactionId: transactionId));
        return this;
    }

    #endregion

    #region Business Logic - Capture

    /// <summary>
    /// Marks the payment as captured (completed), setting the transaction ID and state.
    /// </summary>
    public ErrorOr<Payment> MarkAsCaptured(string? transactionId = null)
    {
        if (State == PaymentState.Completed) return this;
        
        // Capture can only happen from Authorized or immediate Pending (if auto-capture)
        if (State != PaymentState.Authorized && State != PaymentState.Pending)
            return Errors.InvalidStateTransition(from: State, to: PaymentState.Completed);

        ReferenceTransactionId = transactionId ?? ReferenceTransactionId;
        
        if (string.IsNullOrEmpty(ReferenceTransactionId))
            return Errors.ReferenceTransactionIdRequired;

        State = PaymentState.Completed;
        CapturedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(domainEvent: new Events.PaymentCaptured(PaymentId: Id, OrderId: OrderId, ReferenceTransactionId: ReferenceTransactionId!));
        return this;
    }

    #endregion

    #region Business Logic - Void
    /// <summary>
    /// Voids an authorized (but not yet captured) payment.
    /// </summary>
    public ErrorOr<Updated> Void()
    {
        if (State == PaymentState.Void) return Result.Updated;
        
        // Cannot void captured payments
        if (State == PaymentState.Completed) return Errors.CannotVoidCaptured;
        
        if (State != PaymentState.Authorized && State != PaymentState.Pending)
            return Errors.InvalidStateTransition(from: State, to: PaymentState.Void);

        State = PaymentState.Void;
        VoidedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(domainEvent: new Events.PaymentVoided(PaymentId: Id, OrderId: OrderId, ReferenceTransactionId: ReferenceTransactionId));
        return Result.Updated;
    }
    #endregion

    #region Business Logic - Mark as Pending

    /// <summary>
    /// Marks the payment as pending, optionally providing an error message.
    /// This is typically used when a payment requires further action or its status is unknown.
    /// </summary>
    /// <param name="errorMessage">Optional message indicating why the payment is pending.</param>
    public ErrorOr<Payment> MarkAsPending(string? errorMessage = null)
    {
        if (State == PaymentState.Pending) return this;

        State = PaymentState.Pending;
        FailureReason = errorMessage;
        UpdatedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(domainEvent: new Events.PaymentPending(PaymentId: Id, OrderId: OrderId, ErrorMessage: errorMessage ?? "Payment requires action."));
        return this;
    }

    #endregion

    #region Business Logic - Mark as Failed

    /// <summary>
    /// Marks the payment as failed, optionally providing an error message and gateway error code.
    /// </summary>
    /// <param name="errorMessage">Optional message indicating why the payment failed.</param>
    /// <param name="gatewayErrorCode">Optional error code from the payment gateway.</param>
    public ErrorOr<Payment> MarkAsFailed(string errorMessage, string? gatewayErrorCode = null)
    {
        if (State == PaymentState.Failed) return this;

        if (errorMessage.Length > Constraints.FailureReasonMaxLength)
            return Errors.FailureReasonTooLong;
        if (!string.IsNullOrEmpty(value: gatewayErrorCode) && gatewayErrorCode.Length > Constraints.GatewayErrorCodeMaxLength)
            return Errors.GatewayErrorCodeTooLong;

        State = PaymentState.Failed;
        FailureReason = errorMessage;
        GatewayErrorCode = gatewayErrorCode;
        UpdatedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(domainEvent: new Events.PaymentFailed(PaymentId: Id, OrderId: OrderId, ErrorMessage: errorMessage, GatewayErrorCode: gatewayErrorCode));
        return this;
    }

    #endregion

    #region Business Logic - Requires Action
    /// <summary>
    /// Marks the payment as requiring action, such as 3D Secure authentication or redirection.
    /// </summary>
    /// <param name="transactionId">The transaction ID from the payment gateway.</param>
    /// <param name="rawResponse">Raw response from the gateway containing action details (e.g., redirect URL, client secret).</param>
    public ErrorOr<Payment> MarkAsRequiresAction(string transactionId, Dictionary<string, string>? rawResponse = null)
    {
        if (State == PaymentState.RequiresAction) return this;

        ReferenceTransactionId = transactionId;
        State = PaymentState.RequiresAction;
        UpdatedAt = DateTimeOffset.UtcNow;

        if (rawResponse != null)
        {
            foreach (var kvp in rawResponse)
            {
                this.SetPublic(kvp.Key, kvp.Value);
            }
        }

        AddDomainEvent(domainEvent: new Events.PaymentRequiresAction(PaymentId: Id, OrderId: OrderId, ReferenceTransactionId: transactionId, RawResponse: rawResponse));
        return this;
    }
    #endregion

    #region Business Logic - Refund

    /// <summary>
    /// Records a refund for a portion or all of a captured payment.
    /// This method updates the payment's state and records the refunded amount.
    /// </summary>
    /// <param name="amountCents">The amount to refund in cents. Must be positive.</param>
    /// <param name="reason">The reason for the refund.</param>
    /// <param name="referenceTransactionId">Optional transaction ID for the refund from the gateway.</param>
    /// <returns>An ErrorOr result indicating success or failure.</returns>
    public ErrorOr<Payment> Refund(decimal amountCents, string reason, string? referenceTransactionId = null)
    {
        if (amountCents <= 0) return Errors.InvalidAmountCents;

        State = PaymentState.Refunded;
        ReferenceTransactionId = referenceTransactionId ?? ReferenceTransactionId;
        UpdatedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(domainEvent: new Events.PaymentRefunded(PaymentId: Id, OrderId: OrderId, RefundAmountCents: amountCents, ReferenceTransactionId: ReferenceTransactionId, Reason: reason));
        return this;
    }

    #endregion

    #region Events
    public static class Events
    {
        public sealed record PaymentCreated(Guid PaymentId, Guid OrderId, Guid? StoreId = null, string? IdempotencyKey = null) : DomainEvent;
        public sealed record PaymentAuthorized(Guid PaymentId, Guid OrderId, string ReferenceTransactionId) : DomainEvent;
        public sealed record PaymentCapturing(Guid PaymentId, Guid OrderId) : DomainEvent;
        public sealed record PaymentCaptured(Guid PaymentId, Guid OrderId, string ReferenceTransactionId) : DomainEvent;
        public sealed record PaymentVoided(Guid PaymentId, Guid OrderId, string? ReferenceTransactionId) : DomainEvent;
        public sealed record PaymentFailed(Guid PaymentId, Guid OrderId, string ErrorMessage, string? GatewayErrorCode) : DomainEvent;
        public sealed record PaymentPending(Guid PaymentId, Guid OrderId, string ErrorMessage) : DomainEvent;
        public sealed record PaymentRequiresAction(Guid PaymentId, Guid OrderId, string ReferenceTransactionId, Dictionary<string, string>? RawResponse) : DomainEvent;
        public sealed record PaymentRefunded(Guid PaymentId, Guid OrderId, decimal RefundAmountCents, string? ReferenceTransactionId, string Reason) : DomainEvent;
    }
    #endregion
}
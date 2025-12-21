namespace ReSys.Shop.Core.Common.Domain.Shared;

public sealed class Money : ValueObject
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public static Money Create(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required.");

        return new Money(amount, currency);
    }

    /// <summary>
    /// Returns the amount in the smallest unit (e.g., cents for USD).
    /// </summary>
    public long ToMinorUnit()
    {
        // For simplicity, we assume 2 decimal places for most currencies.
        // In production, this would use a currency registry to look up decimals.
        return (long)Math.Round(Amount * 100, MidpointRounding.AwayFromZero);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:F2} {Currency}";
}

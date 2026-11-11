namespace Pricing;

/// <summary>
/// An amount in a single, implicit currency, always held at two decimal places.
/// Midpoints round away from zero (2.345 becomes 2.35), which is the rule the
/// pricing tests state. A production type would also carry a currency.
/// </summary>
public readonly record struct Money
{
    public decimal Amount { get; }

    private Money(decimal amount) => Amount = amount;

    public static Money Zero => new(0m);

    public static Money FromDecimal(decimal amount) =>
        new(Math.Round(amount, 2, MidpointRounding.AwayFromZero));

    public Money Percent(decimal rate) => FromDecimal(Amount * rate);

    public static Money operator +(Money left, Money right) => new(left.Amount + right.Amount);

    public static Money operator -(Money left, Money right) => new(left.Amount - right.Amount);

    public static Money operator *(Money money, int quantity) => new(money.Amount * quantity);
}

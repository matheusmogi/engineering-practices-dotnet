namespace Pricing;

/// <summary>
/// An amount in one currency, always held at two decimal places. Midpoints
/// round away from zero (2.345 becomes 2.35), the rule the pricing tests state.
/// </summary>
/// <remarks>
/// This was a <c>readonly record struct</c> in Part 1. Once it carried a
/// currency, <c>default(Money)</c> would have been an amount with no currency:
/// a value the type exists to rule out. A class has no such default.
/// Two decimal places suit USD and EUR; currencies with other minor units
/// (JPY has none) would need the scale to come from the currency.
/// </remarks>
public sealed record Money
{
    public decimal Amount { get; }

    public Currency Currency { get; }

    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Of(decimal amount, Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);

        return new Money(Math.Round(amount, 2, MidpointRounding.AwayFromZero), currency);
    }

    public static Money Zero(Currency currency) => Of(0m, currency);

    public Money Percent(Percentage rate) => Of(Amount * rate.Fraction, Currency);

    public static Money operator +(Money left, Money right) =>
        new(left.Amount + right.Amount, CommonCurrency(left, right));

    public static Money operator -(Money left, Money right) =>
        new(left.Amount - right.Amount, CommonCurrency(left, right));

    public static Money operator *(Money money, int quantity) =>
        new(money.Amount * quantity, money.Currency);

    public override string ToString() => $"{Amount:0.00} {Currency}";

    private static Currency CommonCurrency(Money left, Money right) =>
        left.Currency == right.Currency
            ? left.Currency
            : throw new InvalidOperationException(
                $"Cannot combine amounts in {left.Currency} and {right.Currency}.");
}

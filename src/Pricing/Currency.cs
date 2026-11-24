namespace Pricing;

/// <summary>
/// An ISO 4217 alphabetic currency code such as USD, EUR or BRL. Only the
/// shape of the code is checked; whether the code is in circulation is not.
/// </summary>
public sealed record Currency
{
    public string Code { get; }

    private Currency(string code) => Code = code;

    public static Currency Usd { get; } = new("USD");

    public static Currency Eur { get; } = new("EUR");

    public static Currency Of(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var normalized = code.Trim().ToUpperInvariant();
        if (normalized.Length != 3 || !normalized.All(char.IsAsciiLetterUpper))
        {
            throw new ArgumentException($"'{code}' is not a three-letter ISO 4217 code.", nameof(code));
        }

        return new Currency(normalized);
    }

    public override string ToString() => Code;
}

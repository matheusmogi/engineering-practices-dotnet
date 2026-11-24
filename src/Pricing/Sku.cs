namespace Pricing;

/// <summary>
/// A stock-keeping unit code. Codes are compared without regard to case or
/// surrounding spaces, so " kb-01" and "KB-01" are the same product.
/// </summary>
public sealed record Sku
{
    public const int MaxLength = 32;

    public string Code { get; }

    private Sku(string code) => Code = code;

    public static Sku Of(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var normalized = code.Trim().ToUpperInvariant();
        if (normalized.Length > MaxLength)
        {
            throw new ArgumentException($"A SKU has at most {MaxLength} characters.", nameof(code));
        }

        return new Sku(normalized);
    }

    public override string ToString() => Code;
}

using System.Globalization;

namespace Pricing;

/// <summary>
/// A rate between 0% and 100%, held as a fraction (0.0825 is 8.25%).
/// </summary>
/// <remarks>
/// A struct is safe here because its default, 0%, is a valid percentage.
/// Compare <see cref="Money"/>, whose default would not be.
/// </remarks>
public readonly record struct Percentage
{
    public decimal Fraction { get; }

    private Percentage(decimal fraction) => Fraction = fraction;

    public static Percentage Zero => default;

    public static Percentage FromFraction(decimal fraction)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(fraction);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(fraction, 1m);

        return new Percentage(fraction);
    }

    public override string ToString() =>
        (Fraction * 100m).ToString("0.####", CultureInfo.InvariantCulture) + "%";
}

namespace Pricing.Tests;

/// <summary>
/// Rules a value owns by itself, tested directly because no path through the
/// order reaches them: the order rejects a foreign currency before Money's own
/// guard is ever asked, and rates are parsed at the edge, before the domain.
/// </summary>
public class ValueObjectTests
{
    [TestCase(-0.01)]
    [TestCase(1.01)]
    public void Percentages_outside_0_to_100_are_rejected(double fraction)
    {
        Assert.That(
            () => Percentage.FromFraction((decimal)fraction),
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void The_default_percentage_is_zero()
    {
        Assert.That(default(Percentage), Is.EqualTo(Percentage.FromFraction(0m)));
    }

    [Test]
    public void Amounts_in_different_currencies_cannot_be_added()
    {
        var dollars = Money.Of(10m, Currency.Usd);
        var euros = Money.Of(10m, Currency.Eur);

        Assert.That(() => dollars + euros, Throws.InvalidOperationException);
    }

    [TestCase("US")]
    [TestCase("US1")]
    [TestCase("USDT")]
    public void Currency_codes_have_three_letters(string code)
    {
        Assert.That(() => Currency.Of(code), Throws.ArgumentException);
    }

    [Test]
    public void Skus_ignore_case_and_surrounding_spaces()
    {
        Assert.That(Sku.Of(" kb-01 "), Is.EqualTo(Sku.Of("KB-01")));
    }
}

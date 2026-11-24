namespace Pricing.Tests;

/// <summary>
/// Part 1's test list, unchanged in what it states. Part 2 changed how the
/// tests build their inputs (Money carries a currency, SKUs and rates are
/// value objects), not one expected figure.
///   1. The subtotal is quantity times unit price, summed over the lines.
///   2. A percentage discount reduces the subtotal.
///   3. A discount that lands on half a cent rounds up.
///   4. Tax is charged on the discounted amount.
///   5. Tax is rounded once on the order, not per line.
///   6. Quantity must be positive.
///   7. An empty order prices to zero.
/// Part 1's eighth test, rates outside their valid range, moved to
/// ValueObjectTests: a rate can no longer reach the pricing function unchecked.
/// </summary>
public class OrderPricingTests
{
    [Test]
    public void Subtotal_is_quantity_times_unit_price_summed_over_lines()
    {
        var order = Order.Start(Currency.Usd);
        order.Add(Sku.Of("KB-01"), quantity: 2, Usd(49.90m));
        order.Add(Sku.Of("MS-07"), quantity: 1, Usd(19.99m));

        var price = OrderPricing.Price(order, Percentage.Zero, Percentage.Zero);

        Assert.That(price.Subtotal, Is.EqualTo(Usd(119.79m)));
        Assert.That(price.Total, Is.EqualTo(Usd(119.79m)));
    }

    [Test]
    public void Percentage_discount_reduces_the_subtotal()
    {
        var order = Order.Start(Currency.Usd);
        order.Add(Sku.Of("KB-01"), quantity: 1, Usd(200.00m));

        var price = OrderPricing.Price(order, Rate(0.25m), Percentage.Zero);

        Assert.That(price.Discount, Is.EqualTo(Usd(50.00m)));
        Assert.That(price.Total, Is.EqualTo(Usd(150.00m)));
    }

    [Test]
    public void Discount_landing_on_half_a_cent_rounds_up()
    {
        var order = Order.Start(Currency.Usd);
        order.Add(Sku.Of("CB-12"), quantity: 1, Usd(23.45m));

        var price = OrderPricing.Price(order, Rate(0.10m), Percentage.Zero);

        // 10% of 23.45 is exactly 2.345. Math.Round's default (banker's rounding) gives 2.34.
        Assert.That(price.Discount, Is.EqualTo(Usd(2.35m)));
        Assert.That(price.Total, Is.EqualTo(Usd(21.10m)));
    }

    [Test]
    public void Tax_is_charged_on_the_discounted_amount()
    {
        var order = Order.Start(Currency.Usd);
        order.Add(Sku.Of("KB-01"), quantity: 2, Usd(49.90m));
        order.Add(Sku.Of("MS-07"), quantity: 1, Usd(19.99m));

        var price = OrderPricing.Price(order, Rate(0.10m), Rate(0.0825m));

        Assert.That(price.Subtotal, Is.EqualTo(Usd(119.79m)));
        Assert.That(price.Discount, Is.EqualTo(Usd(11.98m)));
        Assert.That(price.Tax, Is.EqualTo(Usd(8.89m)));
        Assert.That(price.Total, Is.EqualTo(Usd(116.70m)));
    }

    [Test]
    public void Tax_is_rounded_once_on_the_order_not_per_line()
    {
        var order = Order.Start(Currency.Usd);
        order.Add(Sku.Of("A"), quantity: 1, Usd(1.99m));
        order.Add(Sku.Of("B"), quantity: 1, Usd(1.99m));
        order.Add(Sku.Of("C"), quantity: 1, Usd(1.99m));

        var price = OrderPricing.Price(order, Percentage.Zero, Rate(0.0825m));

        // Per line: 0.16 x 3 = 0.48. On the order: 5.97 x 8.25% = 0.492525 -> 0.49.
        Assert.That(price.Tax, Is.EqualTo(Usd(0.49m)));
        Assert.That(price.Total, Is.EqualTo(Usd(6.46m)));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Quantity_must_be_positive(int quantity)
    {
        var order = Order.Start(Currency.Usd);

        Assert.That(
            () => order.Add(Sku.Of("KB-01"), quantity, Usd(49.90m)),
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Empty_order_prices_to_zero()
    {
        var price = OrderPricing.Price(Order.Start(Currency.Usd), Rate(0.10m), Rate(0.0825m));

        var zero = Money.Zero(Currency.Usd);
        Assert.That(price, Is.EqualTo(new PriceBreakdown(zero, zero, zero, zero)));
    }

    private static Money Usd(decimal amount) => Money.Of(amount, Currency.Usd);

    private static Percentage Rate(decimal fraction) => Percentage.FromFraction(fraction);
}

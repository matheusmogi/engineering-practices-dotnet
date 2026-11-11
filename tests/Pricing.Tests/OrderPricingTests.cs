namespace Pricing.Tests;

/// <summary>
/// The test list, in the order it was worked through:
///   1. The subtotal is quantity times unit price, summed over the lines.
///   2. A percentage discount reduces the subtotal.
///   3. A discount that lands on half a cent rounds up.
///   4. Tax is charged on the discounted amount.
///   5. Tax is rounded once on the order, not per line.
///   6. Quantity must be positive.
///   7. An empty order prices to zero.            (discovered on the way)
///   8. Rates outside their valid range are rejected. (discovered on the way)
/// Every test goes through OrderPricing.Price. Money, OrderLine and
/// PriceBreakdown have no test classes of their own on purpose.
/// </summary>
public class OrderPricingTests
{
    [Test]
    public void Subtotal_is_quantity_times_unit_price_summed_over_lines()
    {
        var order = new Order();
        order.Add("KB-01", quantity: 2, Money.FromDecimal(49.90m));
        order.Add("MS-07", quantity: 1, Money.FromDecimal(19.99m));

        var price = OrderPricing.Price(order, discountRate: 0m, taxRate: 0m);

        Assert.That(price.Subtotal, Is.EqualTo(Money.FromDecimal(119.79m)));
        Assert.That(price.Total, Is.EqualTo(Money.FromDecimal(119.79m)));
    }

    [Test]
    public void Percentage_discount_reduces_the_subtotal()
    {
        var order = new Order();
        order.Add("KB-01", quantity: 1, Money.FromDecimal(200.00m));

        var price = OrderPricing.Price(order, discountRate: 0.25m, taxRate: 0m);

        Assert.That(price.Discount, Is.EqualTo(Money.FromDecimal(50.00m)));
        Assert.That(price.Total, Is.EqualTo(Money.FromDecimal(150.00m)));
    }

    [Test]
    public void Discount_landing_on_half_a_cent_rounds_up()
    {
        var order = new Order();
        order.Add("CB-12", quantity: 1, Money.FromDecimal(23.45m));

        var price = OrderPricing.Price(order, discountRate: 0.10m, taxRate: 0m);

        // 10% of 23.45 is exactly 2.345. Math.Round's default (banker's rounding) gives 2.34.
        Assert.That(price.Discount, Is.EqualTo(Money.FromDecimal(2.35m)));
        Assert.That(price.Total, Is.EqualTo(Money.FromDecimal(21.10m)));
    }

    [Test]
    public void Tax_is_charged_on_the_discounted_amount()
    {
        var order = new Order();
        order.Add("KB-01", quantity: 2, Money.FromDecimal(49.90m));
        order.Add("MS-07", quantity: 1, Money.FromDecimal(19.99m));

        var price = OrderPricing.Price(order, discountRate: 0.10m, taxRate: 0.0825m);

        Assert.That(price.Subtotal, Is.EqualTo(Money.FromDecimal(119.79m)));
        Assert.That(price.Discount, Is.EqualTo(Money.FromDecimal(11.98m)));
        Assert.That(price.Tax, Is.EqualTo(Money.FromDecimal(8.89m)));
        Assert.That(price.Total, Is.EqualTo(Money.FromDecimal(116.70m)));
    }

    [Test]
    public void Tax_is_rounded_once_on_the_order_not_per_line()
    {
        var order = new Order();
        order.Add("A", quantity: 1, Money.FromDecimal(1.99m));
        order.Add("B", quantity: 1, Money.FromDecimal(1.99m));
        order.Add("C", quantity: 1, Money.FromDecimal(1.99m));

        var price = OrderPricing.Price(order, discountRate: 0m, taxRate: 0.0825m);

        // Per line: 0.16 x 3 = 0.48. On the order: 5.97 x 8.25% = 0.492525 -> 0.49.
        Assert.That(price.Tax, Is.EqualTo(Money.FromDecimal(0.49m)));
        Assert.That(price.Total, Is.EqualTo(Money.FromDecimal(6.46m)));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Quantity_must_be_positive(int quantity)
    {
        var order = new Order();

        Assert.That(
            () => order.Add("KB-01", quantity, Money.FromDecimal(49.90m)),
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Empty_order_prices_to_zero()
    {
        var price = OrderPricing.Price(new Order(), discountRate: 0.10m, taxRate: 0.0825m);

        Assert.That(price, Is.EqualTo(new PriceBreakdown(Money.Zero, Money.Zero, Money.Zero, Money.Zero)));
    }

    [TestCase(-0.01, 0.0825)]
    [TestCase(1.01, 0.0825)]
    [TestCase(0.10, -0.01)]
    public void Rates_outside_their_valid_range_are_rejected(double discountRate, double taxRate)
    {
        var order = new Order();
        order.Add("KB-01", quantity: 1, Money.FromDecimal(49.90m));

        Assert.That(
            () => OrderPricing.Price(order, (decimal)discountRate, (decimal)taxRate),
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }
}

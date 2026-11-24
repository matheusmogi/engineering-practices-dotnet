namespace Pricing.Tests;

/// <summary>
/// The order's invariants: rules that no single value can check because they
/// involve several lines, several calls or the order's state. Each test drives
/// the aggregate through its public methods only.
/// </summary>
public class OrderTests
{
    [Test]
    public void Adding_a_sku_already_on_the_order_increases_that_line()
    {
        var order = Order.Start(Currency.Usd);

        order.Add(Sku.Of("KB-01"), quantity: 2, Usd(49.90m));
        order.Add(Sku.Of(" kb-01 "), quantity: 3, Usd(49.90m));

        Assert.That(order.Lines, Has.Count.EqualTo(1));
        Assert.That(order.Lines[0].Quantity, Is.EqualTo(5));
    }

    [Test]
    public void A_sku_cannot_appear_at_two_unit_prices()
    {
        var order = Order.Start(Currency.Usd);
        order.Add(Sku.Of("KB-01"), quantity: 1, Usd(49.90m));

        Assert.That(
            () => order.Add(Sku.Of("KB-01"), quantity: 1, Usd(44.90m)),
            Throws.InvalidOperationException);
        Assert.That(order.Lines[0].Quantity, Is.EqualTo(1));
    }

    [Test]
    public void Units_per_sku_are_capped_across_separate_adds()
    {
        var order = Order.Start(Currency.Usd);
        order.Add(Sku.Of("KB-01"), quantity: 6, Usd(49.90m));

        // Five units is a valid quantity on its own. It is the sum that breaks the rule.
        Assert.That(
            () => order.Add(Sku.Of("KB-01"), quantity: 5, Usd(49.90m)),
            Throws.InvalidOperationException);
        Assert.That(order.Lines[0].Quantity, Is.EqualTo(6));
    }

    [Test]
    public void Units_up_to_the_cap_are_accepted()
    {
        var order = Order.Start(Currency.Usd);

        order.Add(Sku.Of("KB-01"), quantity: 6, Usd(49.90m));
        order.Add(Sku.Of("KB-01"), quantity: Order.MaxUnitsPerSku - 6, Usd(49.90m));

        Assert.That(order.Lines[0].Quantity, Is.EqualTo(Order.MaxUnitsPerSku));
    }

    [Test]
    public void Every_line_is_in_the_order_currency()
    {
        var order = Order.Start(Currency.Usd);

        Assert.That(
            () => order.Add(Sku.Of("KB-01"), quantity: 1, Money.Of(45.00m, Currency.Eur)),
            Throws.InvalidOperationException);
        Assert.That(order.Lines, Is.Empty);
    }

    [Test]
    public void An_order_with_no_lines_cannot_be_placed()
    {
        var order = Order.Start(Currency.Usd);

        Assert.That(() => order.Place(Percentage.Zero, Percentage.Zero), Throws.InvalidOperationException);
        Assert.That(order.Status, Is.EqualTo(OrderStatus.Draft));
    }

    [Test]
    public void Placing_fixes_the_price_at_the_rates_given()
    {
        var order = Order.Start(Currency.Usd);
        order.Add(Sku.Of("KB-01"), quantity: 2, Usd(49.90m));
        order.Add(Sku.Of("MS-07"), quantity: 1, Usd(19.99m));

        var price = order.Place(Rate(0.10m), Rate(0.0825m));

        Assert.That(order.Status, Is.EqualTo(OrderStatus.Placed));
        Assert.That(order.PlacedPrice, Is.EqualTo(price));
        Assert.That(price.Total, Is.EqualTo(Usd(116.70m)));
    }

    [Test]
    public void A_placed_order_accepts_no_more_lines()
    {
        var order = PlacedOrder();

        Assert.That(
            () => order.Add(Sku.Of("MS-07"), quantity: 1, Usd(19.99m)),
            Throws.InvalidOperationException);
        Assert.That(order.Lines, Has.Count.EqualTo(1));
    }

    [Test]
    public void A_placed_order_cannot_be_repriced()
    {
        var order = PlacedOrder();
        var placedPrice = order.PlacedPrice;

        Assert.That(() => order.Place(Rate(0.50m), Percentage.Zero), Throws.InvalidOperationException);
        Assert.That(order.PlacedPrice, Is.EqualTo(placedPrice));
    }

    [Test]
    public void Lines_cannot_be_changed_through_the_collection()
    {
        var order = Order.Start(Currency.Usd);
        order.Add(Sku.Of("KB-01"), quantity: 1, Usd(49.90m));

        var lines = (ICollection<OrderLine>)order.Lines;

        Assert.That(() => lines.Clear(), Throws.TypeOf<NotSupportedException>());
        Assert.That(order.Lines, Has.Count.EqualTo(1));
    }

    private static Order PlacedOrder()
    {
        var order = Order.Start(Currency.Usd);
        order.Add(Sku.Of("KB-01"), quantity: 1, Usd(49.90m));
        order.Place(Percentage.Zero, Rate(0.0825m));
        return order;
    }

    private static Money Usd(decimal amount) => Money.Of(amount, Currency.Usd);

    private static Percentage Rate(decimal fraction) => Percentage.FromFraction(fraction);
}

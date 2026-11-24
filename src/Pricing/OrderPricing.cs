namespace Pricing;

/// <summary>
/// Pure pricing rules: values in, values out. The function prices a draft for
/// display as readily as it prices an order being placed; deciding when a
/// price becomes binding is <see cref="Order.Place"/>'s job, not this one's.
/// </summary>
public static class OrderPricing
{
    public static PriceBreakdown Price(Order order, Percentage discount, Percentage tax)
    {
        ArgumentNullException.ThrowIfNull(order);

        var subtotal = order.Lines.Aggregate(Money.Zero(order.Currency), (sum, line) => sum + line.Subtotal);
        var discountAmount = subtotal.Percent(discount);
        var taxable = subtotal - discountAmount;
        var taxAmount = taxable.Percent(tax);

        return new PriceBreakdown(subtotal, discountAmount, taxAmount, taxable + taxAmount);
    }
}

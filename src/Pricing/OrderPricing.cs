namespace Pricing;

/// <summary>
/// Pure pricing rules: values in, values out. Where the rates come from is the
/// caller's concern (see <see cref="Checkout.CheckoutHandler"/>).
/// </summary>
public static class OrderPricing
{
    /// <param name="order">The order to price.</param>
    /// <param name="discountRate">Fraction between 0 and 1, for example 0.10 for 10%.</param>
    /// <param name="taxRate">Non-negative fraction, for example 0.0825 for 8.25%.</param>
    public static PriceBreakdown Price(Order order, decimal discountRate, decimal taxRate)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentOutOfRangeException.ThrowIfNegative(discountRate);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(discountRate, 1m);
        ArgumentOutOfRangeException.ThrowIfNegative(taxRate);

        var subtotal = order.Lines.Aggregate(Money.Zero, (sum, line) => sum + line.Subtotal);
        var discount = subtotal.Percent(discountRate);
        var taxable = subtotal - discount;
        var tax = taxable.Percent(taxRate);

        return new PriceBreakdown(subtotal, discount, tax, taxable + tax);
    }
}

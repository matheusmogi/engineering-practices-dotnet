namespace Pricing.Checkout;

/// <summary>
/// The edge of the system. Rate providers return raw decimals; they are parsed
/// into <see cref="Percentage"/> here, once, so nothing inside the domain has
/// to ask whether a rate is in range. Loading and saving the order is left to
/// the caller until a later part adds persistence.
/// </summary>
public sealed class CheckoutHandler(IDiscountPolicy discounts, ITaxRates taxRates)
{
    public async Task<PriceBreakdown> PlaceAsync(
        Order order, string customerId, string region, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(order);

        var discount = Percentage.FromFraction(await discounts.GetRateAsync(customerId, cancellationToken));
        var tax = Percentage.FromFraction(await taxRates.GetRateAsync(region, cancellationToken));

        return order.Place(discount, tax);
    }
}

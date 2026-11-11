namespace Pricing.Checkout;

/// <summary>
/// The edge of the system: fetches rates, then delegates to the pure pricing
/// function. It holds no logic worth a unit test; it belongs under an
/// integration test against real infrastructure.
/// </summary>
public sealed class CheckoutHandler(IDiscountPolicy discounts, ITaxRates taxRates)
{
    public async Task<PriceBreakdown> PriceAsync(
        Order order, string customerId, string region, CancellationToken cancellationToken)
    {
        var discountRate = await discounts.GetRateAsync(customerId, cancellationToken);
        var taxRate = await taxRates.GetRateAsync(region, cancellationToken);

        return OrderPricing.Price(order, discountRate, taxRate);
    }
}

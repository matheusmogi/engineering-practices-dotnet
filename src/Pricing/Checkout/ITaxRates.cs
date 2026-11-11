namespace Pricing.Checkout;

public interface ITaxRates
{
    Task<decimal> GetRateAsync(string region, CancellationToken cancellationToken);
}

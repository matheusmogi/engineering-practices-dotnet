namespace Pricing.Checkout;

public interface IDiscountPolicy
{
    Task<decimal> GetRateAsync(string customerId, CancellationToken cancellationToken);
}

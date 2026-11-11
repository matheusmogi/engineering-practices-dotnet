namespace Pricing;

public sealed record PriceBreakdown(Money Subtotal, Money Discount, Money Tax, Money Total);

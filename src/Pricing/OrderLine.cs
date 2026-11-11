namespace Pricing;

public sealed record OrderLine(string Sku, int Quantity, Money UnitPrice)
{
    public Money Subtotal => UnitPrice * Quantity;
}

namespace Pricing;

/// <summary>
/// A line inside an <see cref="Order"/>. Its identity is its <see cref="Sku"/>,
/// and only within its order. Code outside the aggregate can read a line but
/// has no way to change one.
/// </summary>
public sealed class OrderLine
{
    internal OrderLine(Sku sku, int quantity, Money unitPrice)
    {
        Sku = sku;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Sku Sku { get; }

    public int Quantity { get; private set; }

    public Money UnitPrice { get; }

    public Money Subtotal => UnitPrice * Quantity;

    internal void Increase(int quantity) => Quantity += quantity;
}

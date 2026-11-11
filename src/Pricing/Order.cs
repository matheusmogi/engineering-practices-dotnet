namespace Pricing;

public sealed class Order
{
    private readonly List<OrderLine> _lines = [];

    public IReadOnlyList<OrderLine> Lines => _lines;

    public void Add(string sku, int quantity, Money unitPrice)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        _lines.Add(new OrderLine(sku, quantity, unitPrice));
    }
}

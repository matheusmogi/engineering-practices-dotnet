namespace Pricing;

/// <summary>
/// The order aggregate. Every rule that involves more than one line, or the
/// order's state, is checked here, so no sequence of public calls can leave an
/// order that breaks one:
/// <list type="bullet">
/// <item>Every line is priced in the order's currency.</item>
/// <item>A SKU appears on at most one line, at one unit price.</item>
/// <item>No SKU exceeds <see cref="MaxUnitsPerSku"/> units, however many calls added them.</item>
/// <item>An order is placed with at least one line, and once placed its lines and price are fixed.</item>
/// </list>
/// </summary>
public sealed class Order
{
    public const int MaxUnitsPerSku = 10;

    private readonly List<OrderLine> _lines = [];

    public Order(Guid id, Currency currency)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("An order needs a non-empty identity.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(currency);

        Id = id;
        Currency = currency;
    }

    public static Order Start(Currency currency) => new(Guid.CreateVersion7(), currency);

    public Guid Id { get; }

    public Currency Currency { get; }

    public OrderStatus Status { get; private set; } = OrderStatus.Draft;

    /// <summary>The price fixed when the order was placed; null while it is a draft.</summary>
    public PriceBreakdown? PlacedPrice { get; private set; }

    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();

    public void Add(Sku sku, int quantity, Money unitPrice)
    {
        ArgumentNullException.ThrowIfNull(sku);
        ArgumentNullException.ThrowIfNull(unitPrice);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegative(unitPrice.Amount);
        EnsureDraft();

        if (unitPrice.Currency != Currency)
        {
            throw new InvalidOperationException(
                $"Order {Id} is in {Currency}; a line priced in {unitPrice.Currency} cannot be added.");
        }

        var existing = _lines.Find(line => line.Sku == sku);

        if (existing is not null && existing.UnitPrice != unitPrice)
        {
            throw new InvalidOperationException(
                $"{sku} is already on the order at {existing.UnitPrice}, not {unitPrice}.");
        }

        var unitsAfter = (existing?.Quantity ?? 0) + quantity;
        if (unitsAfter > MaxUnitsPerSku)
        {
            throw new InvalidOperationException(
                $"{sku} would reach {unitsAfter} units; the limit is {MaxUnitsPerSku} per order.");
        }

        if (existing is null)
        {
            _lines.Add(new OrderLine(sku, quantity, unitPrice));
        }
        else
        {
            existing.Increase(quantity);
        }
    }

    public PriceBreakdown Place(Percentage discount, Percentage tax)
    {
        EnsureDraft();

        if (_lines.Count == 0)
        {
            throw new InvalidOperationException($"Order {Id} has no lines and cannot be placed.");
        }

        PlacedPrice = OrderPricing.Price(this, discount, tax);
        Status = OrderStatus.Placed;

        return PlacedPrice;
    }

    private void EnsureDraft()
    {
        if (Status != OrderStatus.Draft)
        {
            throw new InvalidOperationException($"Order {Id} is {Status}; it can no longer change.");
        }
    }
}

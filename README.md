# Engineering Practices in .NET

Companion code for a series of articles on software engineering practices that
hold up under real constraints: TDD, Domain-Driven Design, Spec-Driven
Development and AI-assisted engineering. One small domain, order pricing,
evolves through the whole series so each article's claims can be checked
against running code.

## Part 1: TDD as a design tool

Article: [TDD Is a Design Tool That Happens to Leave Tests Behind](https://medium.com/@matheusreis_50439/tdd-is-a-design-tool-that-happens-to-leave-tests-behind-3daa5d4c53df)

The article argues that the main output of test-driven development is design
feedback, and that the regression suite is a by-product. This repository holds
the result of driving an order-pricing feature from a list of behaviours
instead of from a design chosen up front.

What to look at:

| File | Why it matters |
|---|---|
| `tests/Pricing.Tests/OrderPricingTests.cs` | The test list, and one test per behaviour. Every test goes through a single entry point. |
| `src/Pricing/OrderPricing.cs` | The pricing rules as a pure function: values in, values out, no interfaces. |
| `src/Pricing/Money.cs` | Extracted during refactoring. Owns the rounding rule and has no test class of its own. |
| `src/Pricing/Checkout/CheckoutHandler.cs` | Where the I/O went: rates are fetched at the edge and passed in as values. |

Two decisions the tests forced before any production code existed:

- **Rounding.** `Math.Round(value, 2)` uses banker's rounding by default, so a
  10% discount on 23.45 comes back as 2.34. The test states 2.35, and `Money`
  rounds midpoints away from zero.
- **Tax.** Rounding tax per line or once per order differs by a cent on the
  same order (6.45 or 6.46). This repository rounds once per order. Which is
  right for you depends on the tax rules you operate under.

## Part 2: DDD without the ceremony

Article: _link to be added on publication_

Four new business rules (one currency per order, one line and one price per
SKU, at most ten units per SKU, placed orders are fixed) no longer fit in a
pure pricing function, because they are about which states an order may be in
and which changes are allowed. The article sorts each rule by what the code
that checks it needs to see, and lets that decide where it lives.

What to look at:

| File | Why it matters |
|---|---|
| `src/Pricing/Order.cs` | The aggregate. Every rule involving several lines, several calls or the order's state, all checked before the first mutation. |
| `src/Pricing/Percentage.cs` | A value object that stays a struct because its default, 0%, is valid. |
| `src/Pricing/Money.cs` | Became a class once it carried a currency, because `default(Money)` would have had none. |
| `src/Pricing/Checkout/CheckoutHandler.cs` | Raw rates are parsed into `Percentage` here, once, at the edge. |
| `tests/Pricing.Tests/OrderTests.cs` | One test per invariant, each also checking that a rejected change leaves the order untouched. |

Part 1's pricing tests kept every expected figure; only the way they build
their inputs changed. No base classes, repositories, domain events or mediator
were added: none of these rules needed them.

## Running it

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet test EngineeringPractices.slnx
```

## Scope and simplifications

- `Money` assumes two decimal places, which suits USD and EUR but not every
  currency (JPY has none).
- Discount and tax are single rates. Real pricing has stacked promotions and
  per-item tax categories; later parts of the series deal with that.
- Nothing is persisted yet. An order's invariants only hold in production if
  saving it is atomic and rejects stale writes (an optimistic concurrency
  token); a later part adds persistence and an integration test for
  `CheckoutHandler`.

## License

MIT

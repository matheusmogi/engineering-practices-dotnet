# Engineering Practices in .NET

Companion code for a series of articles on software engineering practices that
hold up under real constraints: TDD, Domain-Driven Design, Spec-Driven
Development and AI-assisted engineering. One small domain, order pricing,
evolves through the whole series so each article's claims can be checked
against running code.

## Part 1: TDD as a design tool

Article: _link to be added on publication_

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

## Running it

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet test EngineeringPractices.slnx
```

## Scope and simplifications

- `Money` has no currency. A production type would carry one.
- Discount and tax are single rates. Real pricing has stacked promotions and
  per-item tax categories; later parts of the series deal with that.
- `CheckoutHandler` has no test here. It should be covered by an integration
  test against real infrastructure, which a later part adds.

## License

MIT

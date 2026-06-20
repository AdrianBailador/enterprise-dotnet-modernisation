# Enterprise .NET Modernisation — Sample Code

Sample code for the article **"Modernising Enterprise .NET Applications: A Practical Roadmap"**.

## Structure

```
src/
  Legacy.Api/      # Before — .NET 6, static services, sync ADO.NET, no DI
  Modern.Api/      # After  — .NET 10, async, DI, Polly, health checks, Serilog
tests/
  Modern.Api.Tests/  # xUnit + FluentAssertions + NSubstitute
```

## What it demonstrates

| Pattern | Where |
|---------|-------|
| Async all the way down | `OrderService`, `IOrderRepository` |
| Constructor injection | `OrderService(IOrderRepository, ILogger<>)` |
| Domain model with state transitions | `Order.cs` |
| Value object | `Money.cs` |
| Strangler Fig adapter | `LegacyOrderAdapter.cs` |
| Polly v8 resilience pipeline | `Program.cs` — `AddStandardResilienceHandler` |
| Liveness vs. readiness health checks | `Program.cs` — `/health/live` vs `/health/ready` |
| Structured logging | Serilog with `{Property}` syntax |
| Minimal APIs | `OrderEndpoints.cs` |

## Run

```bash
cd src/Modern.Api
dotnet run
```

API available at `http://localhost:5000`. Health checks at `/health/live` and `/health/ready`.

## Test

```bash
dotnet test
```

## Article

[Modernising Enterprise .NET Applications: A Practical Roadmap](https://adrianbailador.github.io/blog/63-modernising-enterprise-dotnet)

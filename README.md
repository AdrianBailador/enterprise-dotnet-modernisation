# Enterprise .NET Modernisation — Sample Code

Sample code for the article **"Modernising Legacy .NET Applications Without a Rewrite"**.

## Structure

```
src/
  Legacy.Api/      # Before — .NET 6, static services, sync ADO.NET, no DI
  Modern.Api/      # After  — .NET 10, async, DI, Polly, health checks, Serilog, EF Core
tests/
  Modern.Api.Tests/  # xUnit + FluentAssertions + NSubstitute + Testcontainers
```

## What it demonstrates

| Pattern | Where |
|---------|-------|
| Async all the way down | `OrderService`, `IOrderRepository` |
| Constructor injection | `OrderService(IOrderRepository, ILogger<>)` |
| Domain model with state transitions | `Order.cs` |
| Value object | `Money.cs` |
| EF6/EDMX → EF Core Code First | `Infrastructure/Ef/AppDbContext.cs` |
| Characterisation tests against a real database | `Infrastructure/OrderRepositoryCharacterisationTests.cs` (Testcontainers) |
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

The app runs against `InMemoryOrderRepository` by default. Swap in `EfOrderRepository` (see the commented registration in `Program.cs`) once you have a SQL Server connection string configured.

## Test

```bash
dotnet test
```

`OrderRepositoryCharacterisationTests` spins up a real SQL Server container via [Testcontainers](https://dotnet.testcontainers.org/) — a running Docker daemon is required for that test class; the rest of the suite uses NSubstitute and runs without Docker.

## Article

[Modernising Legacy .NET Applications Without a Rewrite](https://adrianbailador.github.io/blog/63-modernising-enterprise-dotnet)

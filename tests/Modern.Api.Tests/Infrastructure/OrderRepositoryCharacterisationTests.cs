using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modern.Api.Domain;
using Modern.Api.Infrastructure.Ef;
using Testcontainers.MsSql;

namespace Modern.Api.Tests.Infrastructure;

// Characterisation tests for the EF Core repository, run against a real,
// disposable SQL Server instance instead of a mock — mocks tend to agree
// with whatever you already assumed about the schema, which defeats the point.
public sealed class OrderRepositoryCharacterisationTests : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder().Build();
    private AppDbContext _db = null!;

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(_sqlContainer.GetConnectionString())
            .Options;

        _db = new AppDbContext(options);
        await _db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _sqlContainer.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_ThenGetById_RoundTripsThroughRealSqlServer()
    {
        var repository = new EfOrderRepository(_db);
        var order = Order.Create(1, "Acme Corp", Money.Of(250m));

        await repository.AddAsync(order);
        await repository.SaveChangesAsync();

        var fetched = await repository.GetByIdAsync(1);

        fetched.Should().NotBeNull();
        fetched!.CustomerName.Should().Be("Acme Corp");
        fetched.Total.Amount.Should().Be(250m);
        fetched.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public async Task GetAllAsync_MultipleOrders_ReturnsAllFromTheDatabase()
    {
        var repository = new EfOrderRepository(_db);
        await repository.AddAsync(Order.Create(1, "Acme Corp", Money.Of(100m)));
        await repository.AddAsync(Order.Create(2, "Globex Inc", Money.Of(200m)));
        await repository.SaveChangesAsync();

        var all = await repository.GetAllAsync();

        all.Should().HaveCount(2);
    }

    [Fact]
    public async Task SaveChangesAsync_AfterStatusTransition_PersistsTheNewStatus()
    {
        var repository = new EfOrderRepository(_db);
        var order = Order.Create(1, "Acme Corp", Money.Of(100m));
        await repository.AddAsync(order);
        await repository.SaveChangesAsync();

        order.MarkAsProcessing();
        await repository.SaveChangesAsync();

        var fetched = await repository.GetByIdAsync(1);
        fetched!.Status.Should().Be(OrderStatus.Processing);
    }
}

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Modern.Api.Application;
using Modern.Api.Domain;
using NSubstitute;

namespace Modern.Api.Tests.Application;

public sealed class OrderServiceTests
{
    private readonly IOrderRepository _repository = Substitute.For<IOrderRepository>();
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _sut = new OrderService(_repository, NullLogger<OrderService>.Instance);
    }

    [Fact]
    public async Task GetOrderAsync_ExistingId_ReturnsOrder()
    {
        var order = Order.Create(1, "Acme Corp", Money.Of(100m));
        _repository.GetByIdAsync(1).Returns(order);

        var result = await _sut.GetOrderAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetOrderAsync_MissingId_ReturnsNull()
    {
        _repository.GetByIdAsync(99).Returns((Order?)null);

        var result = await _sut.GetOrderAsync(99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateOrderAsync_ValidData_PersistsAndReturnsOrder()
    {
        var result = await _sut.CreateOrderAsync("Globex Inc", Money.Of(500m));

        result.CustomerName.Should().Be("Globex Inc");
        result.Total.Amount.Should().Be(500m);
        result.Status.Should().Be(OrderStatus.Pending);

        await _repository.Received(1).AddAsync(Arg.Any<Order>());
        await _repository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task AdvanceStatusAsync_ExistingOrder_MovesToProcessing()
    {
        var order = Order.Create(1, "Acme Corp", Money.Of(100m));
        _repository.GetByIdAsync(1).Returns(order);

        var updated = await _sut.AdvanceStatusAsync(1);

        updated.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Processing);
        await _repository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task AdvanceStatusAsync_MissingOrder_ReturnsFalse()
    {
        _repository.GetByIdAsync(99).Returns((Order?)null);

        var updated = await _sut.AdvanceStatusAsync(99);

        updated.Should().BeFalse();
        await _repository.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task CancelOrderAsync_CompletedOrder_Throws()
    {
        var order = Order.Create(1, "Acme Corp", Money.Of(100m));
        order.MarkAsProcessing();
        order.Complete();
        _repository.GetByIdAsync(1).Returns(order);

        var act = async () => await _sut.CancelOrderAsync(1);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}

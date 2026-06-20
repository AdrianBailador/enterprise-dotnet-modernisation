using FluentAssertions;
using Modern.Api.Domain;

namespace Modern.Api.Tests.Domain;

public sealed class OrderTests
{
    [Fact]
    public void Create_WithValidData_ReturnsPendingOrder()
    {
        var order = Order.Create(1, "Acme Corp", Money.Of(100m));

        order.Status.Should().Be(OrderStatus.Pending);
        order.CustomerName.Should().Be("Acme Corp");
        order.Total.Amount.Should().Be(100m);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankCustomerName_Throws(string name)
    {
        var act = () => Order.Create(1, name, Money.Of(100m));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MarkAsProcessing_FromPending_ChangesStatus()
    {
        var order = Order.Create(1, "Acme Corp", Money.Of(100m));
        order.MarkAsProcessing();
        order.Status.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public void MarkAsProcessing_WhenNotPending_Throws()
    {
        var order = Order.Create(1, "Acme Corp", Money.Of(100m));
        order.MarkAsProcessing();

        var act = () => order.MarkAsProcessing();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Processing*");
    }

    [Fact]
    public void Complete_FromProcessing_ChangesStatus()
    {
        var order = Order.Create(1, "Acme Corp", Money.Of(100m));
        order.MarkAsProcessing();
        order.Complete();
        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public void Complete_FromPending_Throws()
    {
        var order = Order.Create(1, "Acme Corp", Money.Of(100m));
        var act = () => order.Complete();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_CompletedOrder_Throws()
    {
        var order = Order.Create(1, "Acme Corp", Money.Of(100m));
        order.MarkAsProcessing();
        order.Complete();

        var act = () => order.Cancel();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*cannot cancel*");
    }

    [Fact]
    public void Cancel_PendingOrder_ChangesStatus()
    {
        var order = Order.Create(1, "Acme Corp", Money.Of(100m));
        order.Cancel();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }
}

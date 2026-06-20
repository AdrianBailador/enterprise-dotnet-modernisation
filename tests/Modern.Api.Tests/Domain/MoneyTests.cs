using FluentAssertions;
using Modern.Api.Domain;

namespace Modern.Api.Tests.Domain;

public sealed class MoneyTests
{
    [Fact]
    public void Of_WithPositiveAmount_Creates()
    {
        var money = Money.Of(42.50m, "USD");
        money.Amount.Should().Be(42.50m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Of_WithNegativeAmount_Throws()
    {
        var act = () => Money.Of(-1m);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Of_WithZero_IsValid()
    {
        var money = Money.Of(0m);
        money.Amount.Should().Be(0m);
    }

    [Fact]
    public void Zero_HasZeroAmount()
    {
        Money.Zero.Amount.Should().Be(0m);
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        Money.Of(1500m, "EUR").ToString().Should().Be("1500.00 EUR");
    }
}

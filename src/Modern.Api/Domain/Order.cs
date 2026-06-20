namespace Modern.Api.Domain;

public sealed class Order
{
    public int Id { get; private set; }
    public string CustomerName { get; private set; }
    public Money Total { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Order() { CustomerName = string.Empty; Total = Money.Zero; }

    public static Order Create(int id, string customerName, Money total)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customerName);

        return new Order
        {
            Id           = id,
            CustomerName = customerName,
            Total        = total,
            Status       = OrderStatus.Pending,
            CreatedAt    = DateTime.UtcNow
        };
    }

    public void MarkAsProcessing()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot move to Processing from {Status}.");
        Status = OrderStatus.Processing;
    }

    public void Complete()
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOperationException(
                $"Cannot complete an order in {Status} status.");
        Status = OrderStatus.Completed;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed order.");
        Status = OrderStatus.Cancelled;
    }
}

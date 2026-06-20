using Microsoft.Extensions.Logging;
using Modern.Api.Domain;

namespace Modern.Api.Application;

public sealed class OrderService(
    IOrderRepository repository,
    ILogger<OrderService> logger)
{
    public async Task<Order?> GetOrderAsync(int id, CancellationToken ct = default)
    {
        logger.LogInformation("Fetching order {OrderId}", id);

        var order = await repository.GetByIdAsync(id, ct);

        if (order is null)
            logger.LogWarning("Order {OrderId} not found", id);

        return order;
    }

    public async Task<IReadOnlyList<Order>> GetAllOrdersAsync(CancellationToken ct = default)
    {
        logger.LogInformation("Fetching all orders");
        return await repository.GetAllAsync(ct);
    }

    public async Task<Order> CreateOrderAsync(
        string customerName, Money total, CancellationToken ct = default)
    {
        var id = Random.Shared.Next(1000, 9999); // In production, use a proper ID strategy
        var order = Order.Create(id, customerName, total);

        await repository.AddAsync(order, ct);
        await repository.SaveChangesAsync(ct);

        logger.LogInformation(
            "Order {OrderId} created for customer {CustomerName} — total {Total}",
            order.Id, customerName, total);

        return order;
    }

    public async Task<bool> AdvanceStatusAsync(int id, CancellationToken ct = default)
    {
        var order = await repository.GetByIdAsync(id, ct);
        if (order is null) return false;

        order.MarkAsProcessing();
        await repository.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderId} moved to Processing", id);
        return true;
    }

    public async Task<bool> CompleteOrderAsync(int id, CancellationToken ct = default)
    {
        var order = await repository.GetByIdAsync(id, ct);
        if (order is null) return false;

        order.Complete();
        await repository.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderId} completed", id);
        return true;
    }

    public async Task<bool> CancelOrderAsync(int id, CancellationToken ct = default)
    {
        var order = await repository.GetByIdAsync(id, ct);
        if (order is null) return false;

        order.Cancel();
        await repository.SaveChangesAsync(ct);

        logger.LogWarning("Order {OrderId} cancelled", id);
        return true;
    }
}

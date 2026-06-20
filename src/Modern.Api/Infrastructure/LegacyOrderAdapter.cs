using Modern.Api.Application;
using Modern.Api.Domain;

namespace Modern.Api.Infrastructure;

// Strangler Fig pattern — wraps the legacy service during migration.
// Replace this with a real repository once the legacy system is retired.
public sealed class LegacyOrderAdapter(
    ILogger<LegacyOrderAdapter> logger) : IOrderRepository
{
    // Simulate what the legacy static service used to return
    private static readonly Dictionary<int, (string Name, decimal Amount, int Status)> _legacyData = new()
    {
        [1] = ("Acme Corp",   1500.00m, 1),
        [2] = ("Globex Inc",   750.00m, 2),
        [3] = ("Initech Ltd", 3200.00m, 3),
    };

    public Task<Order?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        if (!_legacyData.TryGetValue(id, out var raw))
        {
            logger.LogDebug("Legacy adapter: order {OrderId} not found", id);
            return Task.FromResult<Order?>(null);
        }

        var order = Order.Create(id, raw.Name, Money.Of(raw.Amount));

        // Replay legacy status transitions to restore state
        if (raw.Status >= 2) order.MarkAsProcessing();
        if (raw.Status >= 3) order.Complete();

        logger.LogInformation(
            "Legacy adapter: translated order {OrderId} (legacy status {LegacyStatus} → {DomainStatus})",
            id, raw.Status, order.Status);

        return Task.FromResult<Order?>(order);
    }

    public Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default)
    {
        var orders = _legacyData.Select(kv =>
        {
            var o = Order.Create(kv.Key, kv.Value.Name, Money.Of(kv.Value.Amount));
            if (kv.Value.Status >= 2) o.MarkAsProcessing();
            if (kv.Value.Status >= 3) o.Complete();
            return o;
        }).ToList();

        return Task.FromResult<IReadOnlyList<Order>>(orders);
    }

    public Task AddAsync(Order order, CancellationToken ct = default)
        => Task.CompletedTask; // Write-through to the real legacy DB in production

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}

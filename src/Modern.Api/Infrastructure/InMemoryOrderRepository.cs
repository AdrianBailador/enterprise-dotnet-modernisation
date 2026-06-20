using Modern.Api.Application;
using Modern.Api.Domain;
using System.Collections.Concurrent;

namespace Modern.Api.Infrastructure;

// In-memory implementation — swap for EF Core / Dapper in production
public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<int, Order> _store = new();

    public Task<Order?> GetByIdAsync(int id, CancellationToken ct = default)
        => Task.FromResult(_store.TryGetValue(id, out var order) ? order : null);

    public Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Order>>(_store.Values.ToList());

    public Task AddAsync(Order order, CancellationToken ct = default)
    {
        _store[order.Id] = order;
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => Task.CompletedTask; // No-op for in-memory
}

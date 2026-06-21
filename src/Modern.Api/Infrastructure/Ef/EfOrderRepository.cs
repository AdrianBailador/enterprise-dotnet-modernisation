using Microsoft.EntityFrameworkCore;
using Modern.Api.Application;
using Modern.Api.Domain;

namespace Modern.Api.Infrastructure.Ef;

// EF Core implementation — the eventual replacement for InMemoryOrderRepository
// once the Code First model has been verified against the legacy schema.
public sealed class EfOrderRepository(AppDbContext db) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(int id, CancellationToken ct = default)
        => db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default)
        => await db.Orders.ToListAsync(ct);

    public Task AddAsync(Order order, CancellationToken ct = default)
    {
        db.Orders.Add(order);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}

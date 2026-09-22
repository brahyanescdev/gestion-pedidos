using Microsoft.EntityFrameworkCore;
using Pedidos.Application.Ports;
using Pedidos.Domain.Entities;

namespace Pedidos.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly PedidosDbContext _context;

    public OrderRepository(PedidosDbContext context)
    {
        _context = context;
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Orders
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Order>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Orders
            .Include(order => order.Items)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        => await _context.Orders.AddAsync(order, cancellationToken);

    public Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Update(order);
        return Task.CompletedTask;
    }
}

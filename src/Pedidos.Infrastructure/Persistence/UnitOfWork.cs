using Pedidos.Application.Ports;

namespace Pedidos.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly PedidosDbContext _context;

    public UnitOfWork(PedidosDbContext context)
    {
        _context = context;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}

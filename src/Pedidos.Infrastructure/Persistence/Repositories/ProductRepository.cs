using Microsoft.EntityFrameworkCore;
using Pedidos.Application.Ports;
using Pedidos.Domain.Entities;

namespace Pedidos.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly PedidosDbContext _context;

    public ProductRepository(PedidosDbContext context)
    {
        _context = context;
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Products.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        => await _context.Products.Where(product => ids.Contains(product.Id)).ToListAsync(cancellationToken);

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
        => await _context.Products.AddAsync(product, cancellationToken);
}

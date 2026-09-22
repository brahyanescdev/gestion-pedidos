using Pedidos.Domain.Entities;

namespace Pedidos.Application.Ports;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
}

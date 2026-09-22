using Pedidos.Domain.Entities;

namespace Pedidos.Application.Ports;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Customer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
}

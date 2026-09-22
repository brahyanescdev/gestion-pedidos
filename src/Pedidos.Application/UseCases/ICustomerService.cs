using Pedidos.Application.Dtos;

namespace Pedidos.Application.UseCases;

public interface ICustomerService
{
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default);
}

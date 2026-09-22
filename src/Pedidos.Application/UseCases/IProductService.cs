using Pedidos.Application.Dtos;

namespace Pedidos.Application.UseCases;

public interface IProductService
{
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
}

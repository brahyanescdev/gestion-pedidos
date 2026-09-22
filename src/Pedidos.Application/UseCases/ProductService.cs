using Pedidos.Application.Dtos;
using Pedidos.Application.Ports;
using Pedidos.Domain.Entities;

namespace Pedidos.Application.UseCases;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = Product.Create(request.Name, request.Price, request.Stock);
        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(product);
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        return product is null ? null : ToDto(product);
    }

    public async Task<IReadOnlyCollection<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllAsync(cancellationToken);
        return products.Select(ToDto).ToList();
    }

    private static ProductDto ToDto(Product product) => new(product.Id, product.Name, product.Price, product.Stock);
}

using Moq;
using Pedidos.Application.Dtos;
using Pedidos.Application.Ports;
using Pedidos.Application.UseCases;
using Pedidos.Domain.Entities;
using Xunit;

namespace Pedidos.Application.Tests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _service = new ProductService(_productRepository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task CreateAsync_PersistsProductAndSavesChanges()
    {
        var request = new CreateProductRequest("Teclado", 45.90m, 10);

        var result = await _service.CreateAsync(request);

        _productRepository.Verify(repo => repo.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("Teclado", result.Name);
        Assert.Equal(10, result.Stock);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductDoesNotExist_ReturnsNull()
    {
        _productRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedProducts()
    {
        var products = new[] { Product.Create("Teclado", 45.90m, 10), Product.Create("Mouse", 19.50m, 20) };
        _productRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }
}

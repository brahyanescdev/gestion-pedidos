using Pedidos.Domain.Entities;
using Pedidos.Domain.Exceptions;
using Xunit;

namespace Pedidos.Domain.Tests;

public class ProductTests
{
    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var product = Product.Create("Teclado", 45.90m, 10);

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal("Teclado", product.Name);
        Assert.Equal(45.90m, product.Price);
        Assert.Equal(10, product.Stock);
    }

    [Fact]
    public void Constructor_WithNonPositivePrice_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => new Product(Guid.NewGuid(), "Teclado", 0, 10));
    }

    [Fact]
    public void Constructor_WithNegativeStock_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => new Product(Guid.NewGuid(), "Teclado", 10, -1));
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => new Product(Guid.NewGuid(), " ", 10, 1));
    }

    [Fact]
    public void Reserve_WithEnoughStock_DecreasesStock()
    {
        var product = Product.Create("Mouse", 19.50m, 10);

        product.Reserve(3);

        Assert.Equal(7, product.Stock);
    }

    [Fact]
    public void Reserve_WithInsufficientStock_ThrowsInsufficientStockException()
    {
        var product = Product.Create("Mouse", 19.50m, 2);

        var exception = Assert.Throws<InsufficientStockException>(() => product.Reserve(5));

        Assert.Equal(product.Id, exception.ProductId);
        Assert.Equal(5, exception.Requested);
        Assert.Equal(2, exception.Available);
    }

    [Fact]
    public void Reserve_WithNonPositiveQuantity_ThrowsDomainException()
    {
        var product = Product.Create("Mouse", 19.50m, 10);

        Assert.Throws<DomainException>(() => product.Reserve(0));
    }
}

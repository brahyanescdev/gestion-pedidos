using Pedidos.Domain.Entities;
using Pedidos.Domain.Exceptions;
using Xunit;

namespace Pedidos.Domain.Tests;

public class OrderItemTests
{
    [Fact]
    public void Constructor_WithValidData_ComputesSubtotal()
    {
        var item = new OrderItem(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 3, 10m);

        Assert.Equal(30m, item.Subtotal);
    }

    [Fact]
    public void Constructor_WithNonPositiveQuantity_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => new OrderItem(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0, 10m));
    }

    [Fact]
    public void Constructor_WithNonPositiveUnitPrice_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => new OrderItem(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1, 0m));
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => new OrderItem(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), 1, 10m));
    }
}

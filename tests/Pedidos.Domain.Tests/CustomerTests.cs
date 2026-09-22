using Pedidos.Domain.Entities;
using Pedidos.Domain.Exceptions;
using Xunit;

namespace Pedidos.Domain.Tests;

public class CustomerTests
{
    [Fact]
    public void Create_WithValidData_SetsProperties()
    {
        var customer = Customer.Create("Ana Torres", "ana@example.com");

        Assert.NotEqual(Guid.Empty, customer.Id);
        Assert.Equal("Ana Torres", customer.Name);
        Assert.Equal("ana@example.com", customer.Email);
    }

    [Theory]
    [InlineData("", "ana@example.com")]
    [InlineData("   ", "ana@example.com")]
    [InlineData(null, "ana@example.com")]
    public void Constructor_WithInvalidName_ThrowsDomainException(string? name, string email)
    {
        Assert.Throws<DomainException>(() => new Customer(Guid.NewGuid(), name!, email));
    }

    [Theory]
    [InlineData("Ana Torres", "")]
    [InlineData("Ana Torres", "sin-arroba")]
    [InlineData("Ana Torres", null)]
    public void Constructor_WithInvalidEmail_ThrowsDomainException(string name, string? email)
    {
        Assert.Throws<DomainException>(() => new Customer(Guid.NewGuid(), name, email!));
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => new Customer(Guid.Empty, "Ana Torres", "ana@example.com"));
    }
}

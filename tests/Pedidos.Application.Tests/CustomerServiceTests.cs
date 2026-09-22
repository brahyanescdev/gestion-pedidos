using Moq;
using Pedidos.Application.Dtos;
using Pedidos.Application.Ports;
using Pedidos.Application.UseCases;
using Pedidos.Domain.Entities;
using Xunit;

namespace Pedidos.Application.Tests;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _customerRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        _service = new CustomerService(_customerRepository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task CreateAsync_PersistsCustomerAndSavesChanges()
    {
        var request = new CreateCustomerRequest("Ana Torres", "ana@example.com");

        var result = await _service.CreateAsync(request);

        _customerRepository.Verify(repo => repo.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("Ana Torres", result.Name);
        Assert.Equal("ana@example.com", result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerDoesNotExist_ReturnsNull()
    {
        _customerRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerExists_ReturnsDto()
    {
        var customer = Customer.Create("Ana Torres", "ana@example.com");
        _customerRepository
            .Setup(repo => repo.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var result = await _service.GetByIdAsync(customer.Id);

        Assert.NotNull(result);
        Assert.Equal(customer.Id, result!.Id);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedCustomers()
    {
        var customers = new[] { Customer.Create("Ana Torres", "ana@example.com"), Customer.Create("Luis Gómez", "luis@example.com") };
        _customerRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(customers);

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }
}

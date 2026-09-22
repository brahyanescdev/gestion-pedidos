using Moq;
using Pedidos.Application.Dtos;
using Pedidos.Application.Exceptions;
using Pedidos.Application.Ports;
using Pedidos.Application.UseCases;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Exceptions;
using Xunit;

namespace Pedidos.Application.Tests;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepository = new();
    private readonly Mock<ICustomerRepository> _customerRepository = new();
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly Mock<IOrderReportRepository> _orderReportRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _service = new OrderService(
            _orderRepository.Object,
            _customerRepository.Object,
            _productRepository.Object,
            _orderReportRepository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task CreateAsync_WithNoItems_ThrowsDomainException()
    {
        var request = new CreateOrderRequest(Guid.NewGuid(), Array.Empty<CreateOrderItemRequest>());

        await Assert.ThrowsAsync<DomainException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_WhenCustomerDoesNotExist_ThrowsNotFoundException()
    {
        _customerRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var request = new CreateOrderRequest(Guid.NewGuid(), new[] { new CreateOrderItemRequest(Guid.NewGuid(), 1) });

        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_WhenProductDoesNotExist_ThrowsNotFoundException()
    {
        var customer = Customer.Create("Ana Torres", "ana@example.com");
        _customerRepository
            .Setup(repo => repo.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _productRepository
            .Setup(repo => repo.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Product>());

        var request = new CreateOrderRequest(customer.Id, new[] { new CreateOrderItemRequest(Guid.NewGuid(), 1) });

        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_WhenStockIsInsufficient_ThrowsInsufficientStockException()
    {
        var customer = Customer.Create("Ana Torres", "ana@example.com");
        var product = Product.Create("Teclado", 45.90m, 1);

        _customerRepository
            .Setup(repo => repo.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _productRepository
            .Setup(repo => repo.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { product });

        var request = new CreateOrderRequest(customer.Id, new[] { new CreateOrderItemRequest(product.Id, 5) });

        await Assert.ThrowsAsync<InsufficientStockException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_CreatesOrderAndUpdatesStock()
    {
        var customer = Customer.Create("Ana Torres", "ana@example.com");
        var product = Product.Create("Teclado", 45.90m, 10);

        _customerRepository
            .Setup(repo => repo.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _productRepository
            .Setup(repo => repo.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { product });

        var request = new CreateOrderRequest(customer.Id, new[] { new CreateOrderItemRequest(product.Id, 3) });

        var result = await _service.CreateAsync(request);

        Assert.Equal(customer.Id, result.CustomerId);
        Assert.Equal(137.70m, result.Total);
        Assert.Equal(7, product.Stock);
        _productRepository.Verify(repo => repo.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        _orderRepository.Verify(repo => repo.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderDoesNotExist_ReturnsNull()
    {
        _orderRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedOrders()
    {
        var order = Order.Create(Guid.NewGuid(), DateTime.UtcNow);
        order.AddItem(Guid.NewGuid(), 1, 10m);
        _orderRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { order });

        var result = await _service.GetAllAsync();

        Assert.Single(result);
    }

    [Fact]
    public async Task ConfirmAsync_WhenOrderExists_ConfirmsAndPersists()
    {
        var order = Order.Create(Guid.NewGuid(), DateTime.UtcNow);
        order.AddItem(Guid.NewGuid(), 1, 10m);
        _orderRepository
            .Setup(repo => repo.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _service.ConfirmAsync(order.Id);

        Assert.Equal("Confirmed", result.Status);
        _orderRepository.Verify(repo => repo.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConfirmAsync_WhenOrderDoesNotExist_ThrowsNotFoundException()
    {
        _orderRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.ConfirmAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CancelAsync_WhenOrderExists_CancelsAndPersists()
    {
        var order = Order.Create(Guid.NewGuid(), DateTime.UtcNow);
        _orderRepository
            .Setup(repo => repo.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _service.CancelAsync(order.Id);

        Assert.Equal("Cancelled", result.Status);
        _orderRepository.Verify(repo => repo.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCustomerSummaryAsync_DelegatesToReportRepository()
    {
        var customerId = Guid.NewGuid();
        var summary = new OrderSummaryDto(customerId, 2, 100m, DateTime.UtcNow);
        _orderReportRepository
            .Setup(repo => repo.GetCustomerOrderSummaryAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        var result = await _service.GetCustomerSummaryAsync(customerId);

        Assert.Equal(summary, result);
    }
}

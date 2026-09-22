using Pedidos.Application.Dtos;
using Pedidos.Application.Exceptions;
using Pedidos.Application.Ports;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Exceptions;

namespace Pedidos.Application.UseCases;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderReportRepository _orderReportRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IOrderReportRepository orderReportRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _orderReportRepository = orderReportRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
        {
            throw new DomainException("Un pedido debe incluir al menos un producto.");
        }

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), request.CustomerId);

        var productIds = request.Items.Select(item => item.ProductId).Distinct().ToList();
        var products = await _productRepository.GetByIdsAsync(productIds, cancellationToken);
        var productsById = products.ToDictionary(product => product.Id);

        foreach (var item in request.Items)
        {
            if (!productsById.ContainsKey(item.ProductId))
            {
                throw new NotFoundException(nameof(Product), item.ProductId);
            }
        }

        var order = Order.Create(customer.Id, DateTime.UtcNow);

        foreach (var item in request.Items)
        {
            var product = productsById[item.ProductId];
            product.Reserve(item.Quantity);
            order.AddItem(product.Id, item.Quantity, product.Price);
        }

        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(order);
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : ToDto(order);
    }

    public async Task<IReadOnlyCollection<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);
        return orders.Select(ToDto).ToList();
    }

    public async Task<OrderDto> ConfirmAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);

        order.Confirm();
        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(order);
    }

    public async Task<OrderDto> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);

        order.Cancel();

        foreach (var item in order.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            product?.Release(item.Quantity);
        }

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(order);
    }

    public async Task<OrderDto> CompleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);

        order.Complete();
        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(order);
    }

    public Task<OrderSummaryDto?> GetCustomerSummaryAsync(Guid customerId, CancellationToken cancellationToken = default)
        => _orderReportRepository.GetCustomerOrderSummaryAsync(customerId, cancellationToken);

    private static OrderDto ToDto(Order order) => new(
        order.Id,
        order.CustomerId,
        order.OrderDate,
        order.Status.ToString(),
        order.Total,
        order.Items.Select(item => new OrderItemDto(item.ProductId, item.Quantity, item.UnitPrice, item.Subtotal)).ToList());
}

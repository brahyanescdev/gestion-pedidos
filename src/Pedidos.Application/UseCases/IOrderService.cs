using Pedidos.Application.Dtos;

namespace Pedidos.Application.UseCases;

public interface IOrderService
{
    Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OrderDto> ConfirmAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderDto> CancelAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderSummaryDto?> GetCustomerSummaryAsync(Guid customerId, CancellationToken cancellationToken = default);
}

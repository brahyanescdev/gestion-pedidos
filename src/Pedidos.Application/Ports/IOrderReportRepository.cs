using Pedidos.Application.Dtos;

namespace Pedidos.Application.Ports;

public interface IOrderReportRepository
{
    Task<OrderSummaryDto?> GetCustomerOrderSummaryAsync(Guid customerId, CancellationToken cancellationToken = default);
}

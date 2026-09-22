namespace Pedidos.Application.Dtos;

public record OrderItemDto(Guid ProductId, int Quantity, decimal UnitPrice, decimal Subtotal);

public record OrderDto(
    Guid Id,
    Guid CustomerId,
    DateTime OrderDate,
    string Status,
    decimal Total,
    IReadOnlyCollection<OrderItemDto> Items);

public record CreateOrderItemRequest(Guid ProductId, int Quantity);

public record CreateOrderRequest(Guid CustomerId, IReadOnlyCollection<CreateOrderItemRequest> Items);

public record OrderSummaryDto(Guid CustomerId, int TotalOrders, decimal TotalSpent, DateTime? LastOrderDate);

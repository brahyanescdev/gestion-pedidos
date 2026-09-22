using Pedidos.Domain.Exceptions;

namespace Pedidos.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Subtotal => Quantity * UnitPrice;

    private OrderItem()
    {
    }

    public OrderItem(Guid id, Guid orderId, Guid productId, int quantity, decimal unitPrice)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("El identificador del detalle de pedido es obligatorio.");
        }

        if (quantity <= 0)
        {
            throw new DomainException("La cantidad de un detalle de pedido debe ser mayor a cero.");
        }

        if (unitPrice <= 0)
        {
            throw new DomainException("El precio unitario de un detalle de pedido debe ser mayor a cero.");
        }

        Id = id;
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}

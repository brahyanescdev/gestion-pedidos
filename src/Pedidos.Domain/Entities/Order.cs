using Pedidos.Domain.Enums;
using Pedidos.Domain.Exceptions;

namespace Pedidos.Domain.Entities;

public class Order
{
    private readonly List<OrderItem> _items = new();

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public decimal Total => _items.Sum(item => item.Subtotal);

    private Order()
    {
    }

    private Order(Guid id, Guid customerId, DateTime orderDate)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("El identificador del pedido es obligatorio.");
        }

        if (customerId == Guid.Empty)
        {
            throw new DomainException("El pedido debe estar asociado a un cliente.");
        }

        Id = id;
        CustomerId = customerId;
        OrderDate = orderDate;
        Status = OrderStatus.Pending;
    }

    public static Order Create(Guid customerId, DateTime orderDate) => new(Guid.NewGuid(), customerId, orderDate);

    public static Order Reconstitute(Guid id, Guid customerId, DateTime orderDate, OrderStatus status, IEnumerable<OrderItem> items)
    {
        var order = new Order(id, customerId, orderDate) { Status = status };
        order._items.AddRange(items);
        return order;
    }

    public void AddItem(Guid productId, int quantity, decimal unitPrice)
    {
        if (Status != OrderStatus.Pending)
        {
            throw new DomainException("Sólo se pueden agregar productos a un pedido pendiente.");
        }

        _items.Add(new OrderItem(Guid.NewGuid(), Id, productId, quantity, unitPrice));
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new DomainException("Sólo un pedido pendiente puede confirmarse.");
        }

        if (_items.Count == 0)
        {
            throw new DomainException("Un pedido debe tener al menos un producto para confirmarse.");
        }

        Status = OrderStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Completed)
        {
            throw new DomainException("Un pedido completado no puede cancelarse.");
        }

        Status = OrderStatus.Cancelled;
    }

    public void Complete()
    {
        if (Status != OrderStatus.Confirmed)
        {
            throw new DomainException("Sólo un pedido confirmado puede completarse.");
        }

        Status = OrderStatus.Completed;
    }
}

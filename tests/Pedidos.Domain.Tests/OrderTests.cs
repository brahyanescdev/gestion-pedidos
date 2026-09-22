using Pedidos.Domain.Entities;
using Pedidos.Domain.Enums;
using Pedidos.Domain.Exceptions;
using Xunit;

namespace Pedidos.Domain.Tests;

public class OrderTests
{
    [Fact]
    public void Create_SetsPendingStatusAndNoItems()
    {
        var order = Order.Create(Guid.NewGuid(), DateTime.UtcNow);

        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Empty(order.Items);
        Assert.Equal(0m, order.Total);
    }

    [Fact]
    public void Constructor_WithEmptyCustomerId_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Order.Create(Guid.Empty, DateTime.UtcNow));
    }

    [Fact]
    public void AddItem_AccumulatesTotal()
    {
        var order = Order.Create(Guid.NewGuid(), DateTime.UtcNow);

        order.AddItem(Guid.NewGuid(), 2, 10m);
        order.AddItem(Guid.NewGuid(), 1, 5m);

        Assert.Equal(25m, order.Total);
        Assert.Equal(2, order.Items.Count);
    }

    [Fact]
    public void AddItem_WhenNotPending_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), DateTime.UtcNow);
        order.AddItem(Guid.NewGuid(), 1, 10m);
        order.Confirm();

        Assert.Throws<DomainException>(() => order.AddItem(Guid.NewGuid(), 1, 10m));
    }

    [Fact]
    public void Confirm_WithoutItems_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), DateTime.UtcNow);

        Assert.Throws<DomainException>(() => order.Confirm());
    }

    [Fact]
    public void Confirm_WithItems_SetsConfirmedStatus()
    {
        var order = Order.Create(Guid.NewGuid(), DateTime.UtcNow);
        order.AddItem(Guid.NewGuid(), 1, 10m);

        order.Confirm();

        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), DateTime.UtcNow);
        order.AddItem(Guid.NewGuid(), 1, 10m);
        order.Confirm();

        Assert.Throws<DomainException>(() => order.Confirm());
    }

    [Fact]
    public void Cancel_WhenPending_SetsCancelledStatus()
    {
        var order = Order.Create(Guid.NewGuid(), DateTime.UtcNow);

        order.Cancel();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), DateTime.UtcNow);
        order.Cancel();

        Assert.Throws<DomainException>(() => order.Cancel());
    }

    [Fact]
    public void Cancel_WhenCompleted_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), DateTime.UtcNow);
        order.AddItem(Guid.NewGuid(), 1, 10m);
        order.Confirm();
        order.Complete();

        Assert.Throws<DomainException>(() => order.Cancel());
    }

    [Fact]
    public void Complete_WhenConfirmed_SetsCompletedStatus()
    {
        var order = Order.Create(Guid.NewGuid(), DateTime.UtcNow);
        order.AddItem(Guid.NewGuid(), 1, 10m);
        order.Confirm();

        order.Complete();

        Assert.Equal(OrderStatus.Completed, order.Status);
    }

    [Fact]
    public void Complete_WhenPending_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), DateTime.UtcNow);

        Assert.Throws<DomainException>(() => order.Complete());
    }

    [Fact]
    public void Reconstitute_RebuildsOrderWithItems()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var orderDate = DateTime.UtcNow;
        var items = new[] { new OrderItem(Guid.NewGuid(), orderId, Guid.NewGuid(), 2, 15m) };

        var order = Order.Reconstitute(orderId, customerId, orderDate, OrderStatus.Confirmed, items);

        Assert.Equal(orderId, order.Id);
        Assert.Equal(customerId, order.CustomerId);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
        Assert.Single(order.Items);
        Assert.Equal(30m, order.Total);
    }
}

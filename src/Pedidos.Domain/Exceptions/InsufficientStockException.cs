namespace Pedidos.Domain.Exceptions;

public class InsufficientStockException : DomainException
{
    public InsufficientStockException(Guid productId, int requested, int available)
        : base($"Stock insuficiente para el producto {productId}: solicitado {requested}, disponible {available}.")
    {
        ProductId = productId;
        Requested = requested;
        Available = available;
    }

    public Guid ProductId { get; }
    public int Requested { get; }
    public int Available { get; }
}

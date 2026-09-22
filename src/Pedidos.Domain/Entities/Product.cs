using Pedidos.Domain.Exceptions;

namespace Pedidos.Domain.Entities;

public class Product
{
    public const int MaxNameLength = 200;

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }

    private Product()
    {
        Name = string.Empty;
    }

    public Product(Guid id, string name, decimal price, int stock)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("El identificador del producto es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("El nombre del producto es obligatorio.");
        }

        if (name.Trim().Length > MaxNameLength)
        {
            throw new DomainException($"El nombre del producto no puede superar los {MaxNameLength} caracteres.");
        }

        if (price <= 0)
        {
            throw new DomainException("El precio del producto debe ser mayor a cero.");
        }

        if (stock < 0)
        {
            throw new DomainException("El stock del producto no puede ser negativo.");
        }

        Id = id;
        Name = name.Trim();
        Price = price;
        Stock = stock;
    }

    public static Product Create(string name, decimal price, int stock) => new(Guid.NewGuid(), name, price, stock);

    public void Reserve(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("La cantidad a reservar debe ser mayor a cero.");
        }

        if (quantity > Stock)
        {
            throw new InsufficientStockException(Id, quantity, Stock);
        }

        Stock -= quantity;
    }

    public void Release(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("La cantidad a liberar debe ser mayor a cero.");
        }

        Stock += quantity;
    }
}

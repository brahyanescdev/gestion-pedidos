using Pedidos.Domain.Exceptions;

namespace Pedidos.Domain.Entities;

public class Customer
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }

    private Customer()
    {
        Name = string.Empty;
        Email = string.Empty;
    }

    public Customer(Guid id, string name, string email)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("El identificador del cliente es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("El nombre del cliente es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            throw new DomainException("El correo del cliente no es válido.");
        }

        Id = id;
        Name = name.Trim();
        Email = email.Trim();
    }

    public static Customer Create(string name, string email) => new(Guid.NewGuid(), name, email);
}

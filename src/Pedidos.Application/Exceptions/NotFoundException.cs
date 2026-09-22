namespace Pedidos.Application.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entityName, Guid id)
        : base($"{entityName} con id {id} no fue encontrado.")
    {
    }
}

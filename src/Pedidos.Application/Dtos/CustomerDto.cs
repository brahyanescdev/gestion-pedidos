namespace Pedidos.Application.Dtos;

public record CustomerDto(Guid Id, string Name, string Email);

public record CreateCustomerRequest(string Name, string Email);

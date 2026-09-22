namespace Pedidos.Application.Dtos;

public record ProductDto(Guid Id, string Name, decimal Price, int Stock);

public record CreateProductRequest(string Name, decimal Price, int Stock);

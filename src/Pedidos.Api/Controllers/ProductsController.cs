using Microsoft.AspNetCore.Mvc;
using Pedidos.Application.Dtos;
using Pedidos.Application.UseCases;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProductDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _productService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _productService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }
}

using Microsoft.AspNetCore.Mvc;
using Pedidos.Application.Dtos;
using Pedidos.Application.UseCases;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<OrderDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _orderService.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetByIdAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await _orderService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<ActionResult<OrderDto>> Confirm(Guid id, CancellationToken cancellationToken)
        => Ok(await _orderService.ConfirmAsync(id, cancellationToken));

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<OrderDto>> Cancel(Guid id, CancellationToken cancellationToken)
        => Ok(await _orderService.CancelAsync(id, cancellationToken));

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<OrderDto>> Complete(Guid id, CancellationToken cancellationToken)
        => Ok(await _orderService.CompleteAsync(id, cancellationToken));

    [HttpGet("reports/customer/{customerId:guid}")]
    public async Task<ActionResult<OrderSummaryDto>> GetCustomerSummary(Guid customerId, CancellationToken cancellationToken)
    {
        var summary = await _orderService.GetCustomerSummaryAsync(customerId, cancellationToken);
        return summary is null ? NotFound() : Ok(summary);
    }
}

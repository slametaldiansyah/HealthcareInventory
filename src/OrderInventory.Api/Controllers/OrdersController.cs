using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderInventory.Application.DTOs;
using OrderInventory.Application.Features.Orders.Commands.CancelOrder;
using OrderInventory.Application.Features.Orders.Commands.CreateOrder;
using OrderInventory.Application.Features.Orders.Commands.PayOrder;

namespace OrderInventory.Api.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderResponseDto>> Create(
        [FromBody] CreateOrderRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateOrderCommand(request.UserId, request.Items),
            cancellationToken);

        return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/pay")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderResponseDto>> Pay(
        Guid id,
        [FromBody] PayOrderRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new PayOrderCommand(id, request.PaymentExternalId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderResponseDto>> Cancel(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelOrderCommand(id), cancellationToken);
        return Ok(result);
    }
}

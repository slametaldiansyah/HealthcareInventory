using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderInventory.Application.DTOs;
using OrderInventory.Application.Features.Inventory.Queries.GetInventory;

namespace OrderInventory.Api.Controllers;

[ApiController]
[Route("inventory")]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{sku}")]
    [ProducesResponseType(typeof(InventoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryResponseDto>> Get(
        string sku,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetInventoryQuery(sku), cancellationToken);
        return Ok(result);
    }
}

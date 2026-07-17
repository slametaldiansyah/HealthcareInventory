using MediatR;
using OrderInventory.Application.DTOs;

namespace OrderInventory.Application.Features.Inventory.Queries.GetInventory;

public sealed record GetInventoryQuery(string Sku) : IRequest<InventoryResponseDto>;

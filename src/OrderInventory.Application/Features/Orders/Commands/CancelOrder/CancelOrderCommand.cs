using MediatR;
using OrderInventory.Application.DTOs;

namespace OrderInventory.Application.Features.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId) : IRequest<OrderResponseDto>;

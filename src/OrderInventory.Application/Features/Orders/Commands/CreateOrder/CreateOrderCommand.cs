using MediatR;
using OrderInventory.Application.DTOs;

namespace OrderInventory.Application.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(Guid UserId, IReadOnlyList<OrderItemRequestDto> Items)
    : IRequest<OrderResponseDto>;

using MediatR;
using OrderInventory.Application.DTOs;

namespace OrderInventory.Application.Features.Orders.Commands.PayOrder;

public sealed record PayOrderCommand(Guid OrderId, string PaymentExternalId) : IRequest<OrderResponseDto>;

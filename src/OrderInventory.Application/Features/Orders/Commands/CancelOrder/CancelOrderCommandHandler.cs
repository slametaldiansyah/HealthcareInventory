using AutoMapper;
using MediatR;
using OrderInventory.Application.Common.Interfaces;
using OrderInventory.Application.DTOs;
using OrderInventory.Domain.Enums;
using OrderInventory.Domain.Exceptions;
using OrderInventory.Domain.Interfaces;

namespace OrderInventory.Application.Features.Orders.Commands.CancelOrder;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, OrderResponseDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IOrderEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IInventoryRepository inventoryRepository,
        IOrderEventPublisher eventPublisher,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OrderResponseDto> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        OrderResponseDto? response = null;
        var publishEvent = false;

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var order = await _orderRepository.GetByIdWithDetailsAsync(request.OrderId, ct)
                ?? throw new NotFoundException($"Order '{request.OrderId}' was not found.");

            if (order.Status == OrderStatus.Cancelled)
            {
                response = _mapper.Map<OrderResponseDto>(order);
                response.IdempotentReplay = true;
                return;
            }

            if (order.Status == OrderStatus.Shipped)
            {
                throw new OrderConflictException(
                    $"Order '{order.Id}' cannot be cancelled because it has already been shipped.");
            }

            if (order.Status == OrderStatus.Paid)
            {
                throw new OrderConflictException(
                    $"Order '{order.Id}' is PAID and cannot be cancelled via this endpoint. " +
                    "Refund and restock require a separate compensation flow (out of scope).");
            }

            if (order.Status != OrderStatus.Placed)
            {
                throw new OrderConflictException(
                    $"Order '{order.Id}' cannot be cancelled from status {order.Status.ToString().ToUpperInvariant()}.");
            }

            foreach (var item in order.Items.OrderBy(i => i.Sku))
            {
                var released = await _inventoryRepository.TryReleaseReservationAsync(item.Sku, item.Qty, ct);
                if (!released)
                {
                    throw new OrderConflictException(
                        $"Could not release reserved stock for SKU '{item.Sku}'.");
                }
            }

            order.Status = OrderStatus.Cancelled;
            order.CancelledAtUtc = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);

            response = _mapper.Map<OrderResponseDto>(order);
            publishEvent = true;
        }, cancellationToken);

        if (publishEvent && response is not null)
        {
            await _eventPublisher.PublishOrderCancelledAsync(response.Id, cancellationToken);
        }

        return response
            ?? throw new InvalidOrderException("Cancel completed without a response.");
    }
}

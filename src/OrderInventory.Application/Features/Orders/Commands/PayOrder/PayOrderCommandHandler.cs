using AutoMapper;
using MediatR;
using OrderInventory.Application.Common.Interfaces;
using OrderInventory.Application.DTOs;
using OrderInventory.Domain.Entities;
using OrderInventory.Domain.Enums;
using OrderInventory.Domain.Exceptions;
using OrderInventory.Domain.Interfaces;

namespace OrderInventory.Application.Features.Orders.Commands.PayOrder;

public class PayOrderCommandHandler : IRequestHandler<PayOrderCommand, OrderResponseDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IOrderEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PayOrderCommandHandler(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IInventoryRepository inventoryRepository,
        IPaymentGateway paymentGateway,
        IOrderEventPublisher eventPublisher,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _inventoryRepository = inventoryRepository;
        _paymentGateway = paymentGateway;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OrderResponseDto> Handle(PayOrderCommand request, CancellationToken cancellationToken)
    {
        var externalId = request.PaymentExternalId.Trim();

        var existingPayment = await _paymentRepository.GetByExternalIdAsync(externalId, cancellationToken);
        if (existingPayment is not null)
        {
            if (existingPayment.OrderId != request.OrderId)
            {
                throw new OrderConflictException(
                    $"PaymentExternalId '{externalId}' is already used by another order.");
            }

            var existingOrder = await _orderRepository.GetByIdWithDetailsAsync(request.OrderId, cancellationToken)
                ?? throw new NotFoundException($"Order '{request.OrderId}' was not found.");

            var replay = _mapper.Map<OrderResponseDto>(existingOrder);
            replay.IdempotentReplay = true;
            return replay;
        }

        OrderResponseDto? response = null;
        var publishEvent = false;

        try
        {
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var order = await _orderRepository.GetByIdWithDetailsAsync(request.OrderId, ct)
                    ?? throw new NotFoundException($"Order '{request.OrderId}' was not found.");

                if (order.Status == OrderStatus.Paid
                    && order.Payment?.PaymentExternalId == externalId)
                {
                    response = _mapper.Map<OrderResponseDto>(order);
                    response.IdempotentReplay = true;
                    return;
                }

                if (order.Status != OrderStatus.Placed)
                {
                    throw new OrderConflictException(
                        $"Order '{order.Id}' cannot be paid because its status is {order.Status.ToString().ToUpperInvariant()}.");
                }

                var charged = await _paymentGateway.ChargeAsync(order.Id, externalId, ct);
                if (!charged)
                {
                    throw new InvalidOrderException("Mock payment gateway declined the charge.");
                }

                foreach (var item in order.Items.OrderBy(i => i.Sku))
                {
                    var committed = await _inventoryRepository.TryCommitReservationAsync(item.Sku, item.Qty, ct);
                    if (!committed)
                    {
                        throw new OrderConflictException(
                            $"Could not commit stock reservation for SKU '{item.Sku}'.");
                    }
                }

                var now = DateTime.UtcNow;
                order.Status = OrderStatus.Paid;
                order.PaidAtUtc = now;

                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    PaymentExternalId = externalId,
                    ProcessedAtUtc = now
                };
                await _paymentRepository.AddAsync(payment, ct);
                order.Payment = payment;

                await _unitOfWork.SaveChangesAsync(ct);

                response = _mapper.Map<OrderResponseDto>(order);
                publishEvent = true;
            }, cancellationToken);
        }
        catch (Exception ex) when (ex is ConcurrencyConflictException or DuplicateKeyException)
        {
            var raced = await _paymentRepository.GetByExternalIdAsync(externalId, cancellationToken);
            if (raced is not null && raced.OrderId == request.OrderId)
            {
                var order = await _orderRepository.GetByIdWithDetailsAsync(request.OrderId, cancellationToken)
                    ?? throw new NotFoundException($"Order '{request.OrderId}' was not found.");
                response = _mapper.Map<OrderResponseDto>(order);
                response.IdempotentReplay = true;
                publishEvent = false;
            }
            else if (raced is not null)
            {
                throw new OrderConflictException(
                    $"PaymentExternalId '{externalId}' is already used by another order.");
            }
            else
            {
                var order = await _orderRepository.GetByIdWithDetailsAsync(request.OrderId, cancellationToken);
                if (order?.Status == OrderStatus.Paid)
                {
                    throw new OrderConflictException(
                        $"Order '{request.OrderId}' was already paid with a different PaymentExternalId.");
                }

                throw;
            }
        }

        if (publishEvent && response is not null)
        {
            await _eventPublisher.PublishOrderPaidAsync(response.Id, externalId, cancellationToken);
        }

        return response
            ?? throw new InvalidOrderException("Payment processing completed without a response.");
    }
}

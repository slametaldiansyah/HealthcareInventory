using AutoMapper;
using MediatR;
using OrderInventory.Application.DTOs;
using OrderInventory.Domain.Entities;
using OrderInventory.Domain.Enums;
using OrderInventory.Domain.Exceptions;
using OrderInventory.Domain.Interfaces;

namespace OrderInventory.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponseDto>
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateOrderCommandHandler(
        IInventoryRepository inventoryRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _inventoryRepository = inventoryRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OrderResponseDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var requested = request.Items
            .GroupBy(i => i.Sku.Trim().ToUpperInvariant())
            .Select(g => new { Sku = g.Key, Qty = g.Sum(x => x.Qty) })
            .OrderBy(x => x.Sku)
            .ToList();

        Order? created = null;

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            // Snapshot under lock for validation (all-or-nothing).
            var inventory = await _inventoryRepository.GetBySkusForUpdateAsync(
                requested.Select(r => r.Sku), ct);

            var insufficient = new List<InsufficientStockDetail>();
            foreach (var line in requested)
            {
                if (!inventory.TryGetValue(line.Sku, out var item))
                {
                    insufficient.Add(new InsufficientStockDetail(line.Sku, line.Qty, 0));
                    continue;
                }

                if (item.AvailableQty < line.Qty)
                {
                    insufficient.Add(new InsufficientStockDetail(line.Sku, line.Qty, item.AvailableQty));
                }
            }

            if (insufficient.Count > 0)
            {
                throw new InsufficientStockException(insufficient);
            }

            foreach (var line in requested)
            {
                var reserved = await _inventoryRepository.TryReserveAsync(line.Sku, line.Qty, ct);
                if (!reserved)
                {
                    throw new InsufficientStockException(
                    [
                        new InsufficientStockDetail(line.Sku, line.Qty, inventory[line.Sku].AvailableQty)
                    ]);
                }
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Status = OrderStatus.Placed,
                CreatedAtUtc = DateTime.UtcNow,
                Items = requested.Select(line => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    Sku = line.Sku,
                    Qty = line.Qty
                }).ToList()
            };

            await _orderRepository.AddAsync(order, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            created = order;
        }, cancellationToken);

        return _mapper.Map<OrderResponseDto>(created!);
    }
}

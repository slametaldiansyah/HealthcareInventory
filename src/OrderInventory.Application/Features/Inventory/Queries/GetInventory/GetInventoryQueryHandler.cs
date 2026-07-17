using AutoMapper;
using MediatR;
using OrderInventory.Application.DTOs;
using OrderInventory.Domain.Exceptions;
using OrderInventory.Domain.Interfaces;

namespace OrderInventory.Application.Features.Inventory.Queries.GetInventory;

public class GetInventoryQueryHandler : IRequestHandler<GetInventoryQuery, InventoryResponseDto>
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IMapper _mapper;

    public GetInventoryQueryHandler(IInventoryRepository inventoryRepository, IMapper mapper)
    {
        _inventoryRepository = inventoryRepository;
        _mapper = mapper;
    }

    public async Task<InventoryResponseDto> Handle(GetInventoryQuery request, CancellationToken cancellationToken)
    {
        var sku = request.Sku.Trim().ToUpperInvariant();
        var item = await _inventoryRepository.GetBySkuAsync(sku, cancellationToken)
            ?? throw new NotFoundException($"Inventory SKU '{sku}' was not found.");

        return _mapper.Map<InventoryResponseDto>(item);
    }
}

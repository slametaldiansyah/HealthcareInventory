using AutoMapper;
using OrderInventory.Application.DTOs;
using OrderInventory.Domain.Entities;

namespace OrderInventory.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<OrderItem, OrderItemDto>();
        CreateMap<Order, OrderResponseDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString().ToUpperInvariant()))
            .ForMember(d => d.PaymentExternalId, o => o.MapFrom(s => s.Payment != null ? s.Payment.PaymentExternalId : null))
            .ForMember(d => d.IdempotentReplay, o => o.Ignore());
        CreateMap<InventoryItem, InventoryResponseDto>()
            .ForMember(d => d.AvailableQty, o => o.MapFrom(s => s.AvailableQty));
    }
}

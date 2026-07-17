using Microsoft.Extensions.DependencyInjection;
using OrderInventory.Application.Common.Interfaces;
using OrderInventory.Infrastructure.Events;
using OrderInventory.Infrastructure.Payments;

namespace OrderInventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IOrderEventPublisher, LoggingOrderEventPublisher>();
        services.AddSingleton<IPaymentGateway, MockPaymentGateway>();
        return services;
    }
}

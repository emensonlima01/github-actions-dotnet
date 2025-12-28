using Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace IoC;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddUseCases();

        return services;
    }

    private static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<ReceivePaymentUseCase>();
        services.AddScoped<GetPaymentUseCase>();
        services.AddScoped<CancelPaymentUseCase>();
        services.AddScoped<GetPaymentsPagedUseCase>();

        return services;
    }
}

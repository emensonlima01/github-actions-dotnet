using Domain.Repositories;
using Infrastructure.Configuration;
using Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace IoC;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddMongoDb(configuration)
            .AddRepositories();
        
        return services;
    }

    private static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(
            configuration.GetSection(MongoDbSettings.SectionName));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = configuration.GetSection(MongoDbSettings.SectionName).Get<MongoDbSettings>()
                ?? throw new InvalidOperationException("MongoDbSettings not found in configuration");

            if (string.IsNullOrWhiteSpace(settings.ConnectionString))
                throw new InvalidOperationException("MongoDb connection string not found");

            if (string.IsNullOrWhiteSpace(settings.DatabaseName))
                throw new InvalidOperationException("MongoDb database name not found");

            if (string.IsNullOrWhiteSpace(settings.PaymentsCollectionName))
                throw new InvalidOperationException("MongoDb payments collection name not found");

            return new MongoClient(settings.ConnectionString);
        });
        
        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IPaymentRepository, PaymentRepository>();
        
        return services;
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using EncrypCoin.Application;
using EncrypCoin.Infrastructure;

namespace EncrypCoin.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);

        return services;
    }
}
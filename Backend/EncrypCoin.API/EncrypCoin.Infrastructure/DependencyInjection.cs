using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using EncrypCoin.Infrastructure.Data;
using EncrypCoin.Infrastructure.Repository.Implementations;
using EncrypCoin.Application.Interfaces.Infrastructure;
using EncrypCoin.Application.Interfaces.Application;
using EncrypCoin.Application.Interfaces.External;
using EncrypCoin.Application.Services.Application;
using EncrypCoin.Application.Services.External;

using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace EncrypCoin.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        // 🗄️ Banco
        var connectionString = config.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // 📦 Repository
        services.AddScoped<IUserRepository, UserRepository>();

        // 🔴 Redis
        var redisConnection = config.GetConnectionString("Redis");
        var multiplexer = ConnectionMultiplexer.Connect(redisConnection);

        services.AddSingleton<IConnectionMultiplexer>(multiplexer);
        services.AddScoped<ICacheService, RedisCacheService>();

        // 🌐 HTTP Client
        var baseUrl = config["CoinGecko:BaseUrl"];

        services.AddHttpClient<ICoinGeckoClient, CoinGeckoClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("User-Agent", "EncrypCoinApp/1.0");
        });

        // ❤️ HealthChecks
        services.AddHealthChecks()
            .AddNpgSql(connectionString)
            .AddRedis(redisConnection);

        return services;
    }
}
using EncrypCoin.Application.Interfaces.Application;
using EncrypCoin.Application.Services.Application;

namespace EncrypCoin.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        return services;
    }
}
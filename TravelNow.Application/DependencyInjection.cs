using Microsoft.Extensions.DependencyInjection;

namespace TravelNow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationDI(this IServiceCollection services)
    {
        // Register application services here
        // e.g. services.AddScoped<IUserService, UserService>();

        return services;
    }
}

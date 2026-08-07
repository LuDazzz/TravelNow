using Microsoft.Extensions.DependencyInjection;
using TravelNow.Application.Features.Places.ListPlaces;

namespace TravelNow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationDI(this IServiceCollection services)
    {
        services.AddScoped<ListPlacesHandler>();

        return services;
    }
}

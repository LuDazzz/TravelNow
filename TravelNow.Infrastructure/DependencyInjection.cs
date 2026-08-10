using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TravelNow.Application.Abstractions.Persistence.Places;
using TravelNow.Application.Interfaces.UnitOfWorks;
using TravelNow.Domain.Entities.Identity;
using TravelNow.Infrastructure.Features.Places;
using TravelNow.Infrastructure.UnitOfWorks;

namespace TravelNow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDI(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found. Configure ConnectionStrings:DefaultConnection.");

        services.AddDbContext<TravelNowDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(TravelNowDbContext).Assembly.FullName)));

        // Identity
        services.AddIdentity<User, Role>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;

            options.User.RequireUniqueEmail = true;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
        })
        .AddEntityFrameworkStores<TravelNowDbContext>();

        // UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPlaceReadPort, PlaceReadPort>();

        // HttpContextAccessor
        services.AddHttpContextAccessor();

        return services;
    }
}

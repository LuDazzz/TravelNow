using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TravelNow.Application.Abstractions.Persistence.Places;
using TravelNow.Application.Interfaces;
using TravelNow.Application.Interfaces.UnitOfWorks;
using TravelNow.Domain.Entities.Identity;
using TravelNow.Infrastructure.Features.Auth;
using TravelNow.Infrastructure.Features.Places;
using TravelNow.Infrastructure.UnitOfWorks;

namespace TravelNow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDI(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found. Configure ConnectionStrings:DefaultConnection.");

        services.AddDbContext<TravelNowDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(TravelNowDbContext).Assembly.FullName)));

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

        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
        services.AddSingleton(jwtSettings!);
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        var smtpSettings = configuration.GetSection("SmtpSettings").Get<SmtpSettings>();
        services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
        services.AddSingleton(smtpSettings!);
        services.AddScoped<IMailService, SmtpMailService>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings!.Issuer,
                ValidAudience = jwtSettings!.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
            };
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPlaceReadPort, PlaceReadPort>();
        services.AddHttpContextAccessor();

        return services;
    }

    public static async Task SeedRolesAsync(this IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
        var roles = new[] { "Admin", "Author", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new Role { Name = role });
        }
    }
}
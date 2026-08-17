using Microsoft.Extensions.DependencyInjection;
using TravelNow.Application.Features.Auth.ChangePassword;
using TravelNow.Application.Features.Auth.ForgotPassword;
using TravelNow.Application.Features.Auth.Login;
using TravelNow.Application.Features.Auth.Logout;
using TravelNow.Application.Features.Auth.RefreshToken;
using TravelNow.Application.Features.Auth.Register;
using TravelNow.Application.Features.Auth.ResetPassword;
using TravelNow.Application.Features.Auth.VerifyOtp;
using TravelNow.Application.Features.Places.ListPlaces;

namespace TravelNow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationDI(this IServiceCollection services)
    {
        services.AddScoped<ListPlacesHandler>();
        services.AddScoped<RegisterHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<RefreshTokenHandler>();
        services.AddScoped<ForgotPasswordHandler>();
        services.AddScoped<VerifyOtpHandler>();
        services.AddScoped<ResetPasswordHandler>();
        services.AddScoped<ChangePasswordHandler>();
        services.AddScoped<LogoutHandler>();

        return services;
    }
}
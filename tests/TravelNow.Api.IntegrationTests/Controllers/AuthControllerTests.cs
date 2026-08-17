using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TravelNow.Application.Features.Auth.ChangePassword;
using TravelNow.Application.Features.Auth.ForgotPassword;
using TravelNow.Application.Features.Auth.Login;
using TravelNow.Application.Features.Auth.RefreshToken;
using TravelNow.Application.Features.Auth.Register;
using TravelNow.Application.Features.Auth.ResetPassword;
using TravelNow.Application.Features.Auth.VerifyOtp;
using TravelNow.Application.Interfaces;
using Xunit;

namespace TravelNow.Api.IntegrationTests.Controllers;

public sealed class AuthControllerTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions ReadOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task Register_returns_profile_and_token_pair()
    {
        using var client = CreateClientWithFakeAuth();

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "register@example.com",
            userName = "registeruser",
            password = "Passw0rd",
            firstName = "Reg",
            lastName = "Ister"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<RegisterDto>(ReadOptions);
        Assert.NotNull(body);
        Assert.Equal("register@example.com", body!.Email);
        Assert.Equal("registeruser", body.UserName);
        Assert.Equal("register-access", body.AccessToken);
        Assert.Equal("register-refresh", body.RefreshToken);
    }

    [Fact]
    public async Task Login_returns_profile_roles_and_token_pair()
    {
        using var client = CreateClientWithFakeAuth();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "login@example.com",
            password = "Passw0rd",
            rememberMe = true
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginDto>(ReadOptions);
        Assert.NotNull(body);
        Assert.Equal("login@example.com", body!.Email);
        Assert.Contains("Author", body.Roles);
        Assert.Equal("login-access", body.AccessToken);
        Assert.Equal("login-refresh", body.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_returns_rotated_token_pair()
    {
        using var client = CreateClientWithFakeAuth();

        var response = await client.PostAsJsonAsync("/api/auth/refresh-token", new
        {
            accessToken = "old-access",
            refreshToken = "old-refresh"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<RefreshDto>(ReadOptions);
        Assert.NotNull(body);
        Assert.Equal("refreshed-access", body!.AccessToken);
        Assert.Equal("refreshed-refresh", body.RefreshToken);
    }

    [Fact]
    public async Task ForgotPassword_reports_otp_sent()
    {
        using var client = CreateClientWithFakeAuth();

        var response = await client.PostAsJsonAsync("/api/auth/forgot-password", new
        {
            email = "forgot@example.com"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<MessageDto>(ReadOptions);
        Assert.NotNull(body);
        Assert.Equal("OTP sent to your email.", body!.Message);
    }

    [Fact]
    public async Task ChangePassword_requires_authentication()
    {
        using var client = CreateClientWithFakeAuth();

        var response = await client.PostAsJsonAsync("/api/auth/change-password", new
        {
            currentPassword = "Passw0rd",
            newPassword = "NewPass1",
            confirmPassword = "NewPass1"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_requires_authentication()
    {
        using var client = CreateClientWithFakeAuth();

        var response = await client.PostAsync("/api/auth/logout", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private HttpClient CreateClientWithFakeAuth()
    {
        var testFactory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAuthService>();
                services.AddSingleton<IAuthService>(new FakeAuthService());
            }));

        return testFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    private sealed class FakeAuthService : IAuthService
    {
        public Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken)
            => Task.FromResult(new LoginResult(Guid.NewGuid(), command.Email, "loginuser", ["Author"], "login-access", "login-refresh"));

        public Task<RegisterResult> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken)
            => Task.FromResult(new RegisterResult(Guid.NewGuid(), command.Email, command.UserName, "register-access", "register-refresh"));

        public Task<RefreshTokenResult> RefreshTokenAsync(RefreshTokenCommand command, CancellationToken cancellationToken)
            => Task.FromResult(new RefreshTokenResult("refreshed-access", "refreshed-refresh"));

        public Task LogoutAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<ForgotPasswordResult> ForgotPasswordAsync(ForgotPasswordCommand command, CancellationToken cancellationToken)
            => Task.FromResult(new ForgotPasswordResult("OTP sent to your email."));

        public Task VerifyOtpAsync(VerifyOtpCommand command, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<ResetPasswordResult> ResetPasswordAsync(ResetPasswordCommand command, CancellationToken cancellationToken)
            => Task.FromResult(new ResetPasswordResult("Password reset successfully."));

        public Task ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed record RegisterDto(Guid UserId, string Email, string UserName, string AccessToken, string RefreshToken);
    private sealed record LoginDto(Guid UserId, string Email, string UserName, IList<string> Roles, string AccessToken, string RefreshToken);
    private sealed record RefreshDto(string AccessToken, string RefreshToken);
    private sealed record MessageDto(string Message);
}

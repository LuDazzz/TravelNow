using TravelNow.Application.Interfaces;

namespace TravelNow.Application.Features.Auth.Login;

public sealed class LoginHandler(IAuthService authService)
{
    public async Task<LoginResult> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        return await authService.LoginAsync(command, cancellationToken);
    }
}
using TravelNow.Application.Interfaces;

namespace TravelNow.Application.Features.Auth.Logout;

public sealed class LogoutHandler(IAuthService authService)
{
    public async Task HandleAsync(CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(cancellationToken);
    }
}

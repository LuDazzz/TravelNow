using TravelNow.Application.Interfaces;

namespace TravelNow.Application.Features.Auth.RefreshToken;

public sealed class RefreshTokenHandler(IAuthService authService)
{
    public async Task<RefreshTokenResult> HandleAsync(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        return await authService.RefreshTokenAsync(command, cancellationToken);
    }
}
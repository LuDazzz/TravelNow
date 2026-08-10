using TravelNow.Application.Interfaces;

namespace TravelNow.Application.Features.Auth.ChangePassword;

public sealed class ChangePasswordHandler(IAuthService authService)
{
    public async Task HandleAsync(
        ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await authService.ChangePasswordAsync(command, cancellationToken);
    }
}
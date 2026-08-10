using TravelNow.Application.Interfaces;

namespace TravelNow.Application.Features.Auth.ResetPassword;

public sealed class ResetPasswordHandler(IAuthService authService)
{
    public async Task<ResetPasswordResult> HandleAsync(
        ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        return await authService.ResetPasswordAsync(command, cancellationToken);
    }
}
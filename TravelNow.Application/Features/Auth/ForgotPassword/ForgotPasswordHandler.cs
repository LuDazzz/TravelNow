using TravelNow.Application.Interfaces;

namespace TravelNow.Application.Features.Auth.ForgotPassword;

public sealed class ForgotPasswordHandler(IAuthService authService)
{
    public async Task<ForgotPasswordResult> HandleAsync(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        return await authService.ForgotPasswordAsync(command, cancellationToken);
    }
}
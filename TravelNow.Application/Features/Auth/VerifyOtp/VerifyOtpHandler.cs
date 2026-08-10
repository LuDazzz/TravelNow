using TravelNow.Application.Interfaces;

namespace TravelNow.Application.Features.Auth.VerifyOtp;

public sealed class VerifyOtpHandler(IAuthService authService)
{
    public async Task HandleAsync(
        VerifyOtpCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await authService.VerifyOtpAsync(command, cancellationToken);
    }
}
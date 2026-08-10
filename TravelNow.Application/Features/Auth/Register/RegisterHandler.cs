using TravelNow.Application.Interfaces;

namespace TravelNow.Application.Features.Auth.Register;

public sealed class RegisterHandler(IAuthService authService)
{
    public async Task<RegisterResult> HandleAsync(
        RegisterCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        return await authService.RegisterAsync(command, cancellationToken);
    }
}
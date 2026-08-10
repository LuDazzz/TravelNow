using TravelNow.Application.Features.Auth.ChangePassword;
using TravelNow.Application.Features.Auth.ForgotPassword;
using TravelNow.Application.Features.Auth.Login;
using TravelNow.Application.Features.Auth.RefreshToken;
using TravelNow.Application.Features.Auth.Register;
using TravelNow.Application.Features.Auth.ResetPassword;
using TravelNow.Application.Features.Auth.VerifyOtp;

namespace TravelNow.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken);
    Task<RegisterResult> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken);
    Task<RefreshTokenResult> RefreshTokenAsync(RefreshTokenCommand command, CancellationToken cancellationToken);
    Task LogoutAsync(CancellationToken cancellationToken);
    Task<ForgotPasswordResult> ForgotPasswordAsync(ForgotPasswordCommand command, CancellationToken cancellationToken);
    Task VerifyOtpAsync(VerifyOtpCommand command, CancellationToken cancellationToken);
    Task<ResetPasswordResult> ResetPasswordAsync(ResetPasswordCommand command, CancellationToken cancellationToken);
    Task ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken);
}
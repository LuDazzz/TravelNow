using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelNow.Application.Features.Auth.ChangePassword;
using TravelNow.Application.Features.Auth.ForgotPassword;
using TravelNow.Application.Features.Auth.Login;
using TravelNow.Application.Features.Auth.Logout;
using TravelNow.Application.Features.Auth.RefreshToken;
using TravelNow.Application.Features.Auth.Register;
using TravelNow.Application.Features.Auth.ResetPassword;
using TravelNow.Application.Features.Auth.VerifyOtp;
using TravelNow.Models.Auth;

namespace TravelNow.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    RegisterHandler registerHandler,
    LoginHandler loginHandler,
    RefreshTokenHandler refreshTokenHandler,
    ForgotPasswordHandler forgotPasswordHandler,
    VerifyOtpHandler verifyOtpHandler,
    ResetPasswordHandler resetPasswordHandler,
    ChangePasswordHandler changePasswordHandler,
    LogoutHandler logoutHandler) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand
        {
            Email = request.Email,
            UserName = request.UserName,
            Password = request.Password,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await registerHandler.HandleAsync(command, cancellationToken);

        return Ok(new RegisterResponse(
            result.UserId,
            result.Email,
            result.UserName,
            result.AccessToken,
            result.RefreshToken));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password,
            RememberMe = request.RememberMe
        };

        var result = await loginHandler.HandleAsync(command, cancellationToken);

        return Ok(new LoginResponse(
            result.UserId,
            result.Email,
            result.UserName,
            result.Roles,
            result.AccessToken,
            result.RefreshToken));
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand
        {
            AccessToken = request.AccessToken,
            RefreshToken = request.RefreshToken
        };

        var result = await refreshTokenHandler.HandleAsync(command, cancellationToken);

        return Ok(new RefreshTokenResponse(
            result.AccessToken,
            result.RefreshToken));
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ForgotPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ForgotPasswordCommand
        {
            Email = request.Email
        };

        var result = await forgotPasswordHandler.HandleAsync(command, cancellationToken);

        return Ok(new ForgotPasswordResponse(result.Message));
    }

    [HttpPost("verify-otp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyOtp(
        [FromBody] VerifyOtpRequest request,
        CancellationToken cancellationToken)
    {
        var command = new VerifyOtpCommand
        {
            Email = request.Email,
            Otp = request.Otp
        };

        await verifyOtpHandler.HandleAsync(command, cancellationToken);

        return Ok(new { message = "OTP verified successfully" });
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ResetPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResetPasswordResponse>> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ResetPasswordCommand
        {
            Email = request.Email,
            Otp = request.Otp,
            NewPassword = request.NewPassword,
            ConfirmPassword = request.ConfirmPassword
        };

        var result = await resetPasswordHandler.HandleAsync(command, cancellationToken);

        return Ok(new ResetPasswordResponse(result.Message));
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangePasswordCommand
        {
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword,
            ConfirmPassword = request.ConfirmPassword
        };

        await changePasswordHandler.HandleAsync(command, cancellationToken);

        return Ok(new { message = "Password changed successfully" });
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await logoutHandler.HandleAsync(cancellationToken);

        return Ok(new { message = "Logged out successfully" });
    }
}
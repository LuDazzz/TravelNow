using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TravelNow.Application.Features.Auth.ChangePassword;
using TravelNow.Application.Features.Auth.ForgotPassword;
using TravelNow.Application.Features.Auth.Login;
using TravelNow.Application.Features.Auth.RefreshToken;
using TravelNow.Application.Features.Auth.Register;
using TravelNow.Application.Features.Auth.ResetPassword;
using TravelNow.Application.Features.Auth.VerifyOtp;
using TravelNow.Application.Interfaces;
using TravelNow.Domain.Entities;
using TravelNow.Domain.Entities.Identity;
using TravelNow.Domain.Exceptions;

namespace TravelNow.Infrastructure.Features.Auth;

public sealed class AuthService(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IJwtTokenService jwtTokenService,
    IMailService mailService,
    TravelNowDbContext dbContext,
    IHttpContextAccessor httpContextAccessor) : IAuthService
{
    public async Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(command.Email);

        if (user == null || !user.IsActive)
            throw new BadRequestException("Invalid email or password");

        var result = await signInManager.CheckPasswordSignInAsync(user, command.Password, false);

        if (!result.Succeeded)
            throw new BadRequestException("Invalid email or password");

        var roles = await userManager.GetRolesAsync(user);

        var accessToken = jwtTokenService.GenerateAccessToken(user, roles);
        var refreshToken = jwtTokenService.GenerateRefreshToken();
        var tokenExp = jwtTokenService.GetRefreshTokenExpiry();

        var existing = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.UserId == user.Id && !t.IsRevoked, cancellationToken);

        if (existing != null)
        {
            existing.IsUsed = true;
            existing.IsRevoked = true;
        }

        var token = new RefreshTokenEntity
        {
            UserId = user.Id,
            Token = refreshToken,
            Expires = tokenExp,
            DeviceInfo = command.RememberMe ? "Persistent" : "Session"
        };
        await dbContext.RefreshTokens.AddAsync(token, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new LoginResult(
            user.Id,
            user.Email!,
            user.UserName!,
            roles,
            accessToken,
            refreshToken);
    }

    public async Task<RegisterResult> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken)
    {
        var existingUser = await userManager.FindByEmailAsync(command.Email);
        if (existingUser != null)
            throw new BadRequestException("Email already registered");

        existingUser = await userManager.FindByNameAsync(command.UserName);
        if (existingUser != null)
            throw new BadRequestException("Username already taken");

        var user = new User
        {
            UserName = command.UserName,
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, command.Password);

        if (!result.Succeeded)
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, "Author");

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = jwtTokenService.GenerateAccessToken(user, roles);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        var token = new RefreshTokenEntity
        {
            UserId = user.Id,
            Token = refreshToken,
            Expires = jwtTokenService.GetRefreshTokenExpiry(),
            DeviceInfo = "Session"
        };
        await dbContext.RefreshTokens.AddAsync(token, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new RegisterResult(
            user.Id,
            user.Email!,
            user.UserName!,
            accessToken,
            refreshToken);
    }

    public async Task<RefreshTokenResult> RefreshTokenAsync(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var principal = jwtTokenService.GetPrincipalFromExpiredToken(command.AccessToken);
        var userIdStr = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = new Guid(userIdStr!);

        var storedToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == command.RefreshToken && !t.IsUsed && !t.IsRevoked, cancellationToken);

        if (storedToken == null || storedToken.Expires < DateTimeOffset.UtcNow)
            throw new BadRequestException("Invalid or expired refresh token");

        storedToken.IsUsed = true;
        storedToken.IsRevoked = true;
        await dbContext.SaveChangesAsync(cancellationToken);

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new BadRequestException("Invalid token");

        var roles = await userManager.GetRolesAsync(user);
        var newAccessToken = jwtTokenService.GenerateAccessToken(user, roles);
        var newRefreshToken = jwtTokenService.GenerateRefreshToken();

        var newToken = new RefreshTokenEntity
        {
            UserId = user.Id,
            Token = newRefreshToken,
            Expires = jwtTokenService.GetRefreshTokenExpiry(),
            DeviceInfo = storedToken.DeviceInfo
        };
        await dbContext.RefreshTokens.AddAsync(newToken, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResult(newAccessToken, newRefreshToken);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != null)
        {
            var guid = new Guid(userId);
            var tokens = await dbContext.RefreshTokens
                .Where(t => t.UserId == guid && !t.IsRevoked)
                .ToListAsync(cancellationToken);
            foreach (var t in tokens)
                t.IsRevoked = true;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await signInManager.SignOutAsync();
    }

    public async Task<ForgotPasswordResult> ForgotPasswordAsync(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user == null)
            return new ForgotPasswordResult("If the email exists, an OTP has been sent.");

        var otp = GenerateOtp();
        var otpHash = HashOtp(otp);

        var existingOtp = await dbContext.OtpRequests
            .FirstOrDefaultAsync(t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAt > DateTimeOffset.UtcNow, cancellationToken);

        if (existingOtp != null)
        {
            existingOtp.OtpHash = otpHash;
            existingOtp.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10);
        }
        else
        {
            var otpRequest = new OtpRequest
            {
                UserId = user.Id,
                Email = user.Email!,
                OtpHash = otpHash,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10)
            };
            await dbContext.OtpRequests.AddAsync(otpRequest, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var name = user.FirstName ?? "User";
        await mailService.SendAsync(
            user.Email!,
            "TravelNow - OTP Password Reset",
            $"<!DOCTYPE html><html><body style=\"font-family: Arial, sans-serif; padding: 40px;\">" +
            $"<h2>TravelNow - Password Reset OTP</h2>" +
            $"<p>Hello {name},</p>" +
            $"<p>Your one-time password (OTP) for password reset is:</p>" +
            $"<div style=\"background: #f4f4f4; padding: 20px; text-align: center; font-size: 32px; font-weight: bold; letter-spacing: 8px; border-radius: 8px;\">" +
            $"{otp}</div>" +
            $"<p style=\"color: #888; margin-top: 20px;\">This OTP expires in 10 minutes. If you didn't request this, ignore this email.</p>" +
            "</body></html>");

        return new ForgotPasswordResult("OTP sent to your email.");
    }

    public async Task VerifyOtpAsync(VerifyOtpCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user == null)
            throw new BadRequestException("Invalid email or OTP");

        var otpHash = HashOtp(command.Otp);

        var otpRequest = await dbContext.OtpRequests
            .FirstOrDefaultAsync(
                t => t.UserId == user.Id && t.OtpHash == otpHash && !t.IsUsed && t.ExpiresAt > DateTimeOffset.UtcNow,
                cancellationToken);

        if (otpRequest == null)
            throw new BadRequestException("Invalid or expired OTP");
    }

    public async Task<ResetPasswordResult> ResetPasswordAsync(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user == null)
            throw new BadRequestException("Invalid email or OTP");

        var otpHash = HashOtp(command.Otp);

        var otpRequest = await dbContext.OtpRequests
            .FirstOrDefaultAsync(
                t => t.UserId == user.Id && t.OtpHash == otpHash && !t.IsUsed && t.ExpiresAt > DateTimeOffset.UtcNow,
                cancellationToken);

        if (otpRequest == null)
            throw new BadRequestException("Invalid or expired OTP");

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, command.NewPassword);

        if (!result.Succeeded)
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

        otpRequest.IsUsed = true;
        await dbContext.SaveChangesAsync(cancellationToken);

        await RevokeAllRefreshTokens(user.Id, cancellationToken);

        return new ResetPasswordResult("Password reset successfully.");
    }

    public async Task ChangePasswordAsync(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            throw new UnauthorizedAccessException();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User");

        var result = await userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword);

        if (!result.Succeeded)
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

        await RevokeAllRefreshTokens(user.Id, cancellationToken);
    }

    async Task RevokeAllRefreshTokens(Guid userId, CancellationToken cancellationToken)
    {
        var tokens = await dbContext.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync(cancellationToken);
        foreach (var t in tokens)
            t.IsRevoked = true;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    static string GenerateOtp()
    {
        var rng = RandomNumberGenerator.GetInt32(100000, 999999);
        return rng.ToString();
    }

    static string HashOtp(string otp)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(otp));
        return Convert.ToBase64String(hash);
    }
}
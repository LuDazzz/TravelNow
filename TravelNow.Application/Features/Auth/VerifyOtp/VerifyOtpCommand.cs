using System.ComponentModel.DataAnnotations;

namespace TravelNow.Application.Features.Auth.VerifyOtp;

public sealed class VerifyOtpCommand
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    [StringLength(6, MinimumLength = 6)]
    public required string Otp { get; init; }
}
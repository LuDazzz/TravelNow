using System.ComponentModel.DataAnnotations;

namespace TravelNow.Models.Auth;

public sealed class VerifyOtpRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    [StringLength(6, MinimumLength = 6)]
    public required string Otp { get; init; }
}
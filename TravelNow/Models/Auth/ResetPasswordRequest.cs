using System.ComponentModel.DataAnnotations;

namespace TravelNow.Models.Auth;

public sealed class ResetPasswordRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    [StringLength(6, MinimumLength = 6)]
    public required string Otp { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public required string NewPassword { get; init; }

    [Required]
    [Compare("NewPassword")]
    public required string ConfirmPassword { get; init; }
}
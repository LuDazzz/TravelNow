using System.ComponentModel.DataAnnotations;

namespace TravelNow.Application.Features.Auth.ChangePassword;

public sealed class ChangePasswordCommand
{
    [Required]
    public required string CurrentPassword { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public required string NewPassword { get; init; }

    [Required]
    [Compare("NewPassword")]
    public required string ConfirmPassword { get; init; }
}
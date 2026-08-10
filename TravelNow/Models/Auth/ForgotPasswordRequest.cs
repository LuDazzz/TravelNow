using System.ComponentModel.DataAnnotations;

namespace TravelNow.Models.Auth;

public sealed class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }
}
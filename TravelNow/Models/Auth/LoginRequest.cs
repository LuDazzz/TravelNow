using System.ComponentModel.DataAnnotations;

namespace TravelNow.Models.Auth;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }

    public bool RememberMe { get; init; }
}
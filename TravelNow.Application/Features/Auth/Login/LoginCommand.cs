using System.ComponentModel.DataAnnotations;

namespace TravelNow.Application.Features.Auth.Login;

public sealed class LoginCommand
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }

    public bool RememberMe { get; init; }
}
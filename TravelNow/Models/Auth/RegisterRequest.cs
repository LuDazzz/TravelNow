using System.ComponentModel.DataAnnotations;

namespace TravelNow.Models.Auth;

public sealed class RegisterRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    [StringLength(100)]
    public required string UserName { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public required string Password { get; init; }

    [StringLength(100)]
    public string? FirstName { get; init; }

    [StringLength(100)]
    public string? LastName { get; init; }
}
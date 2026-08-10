using System.ComponentModel.DataAnnotations;

namespace TravelNow.Models.Auth;

public sealed class RefreshTokenRequest
{
    [Required]
    public required string AccessToken { get; init; }

    [Required]
    public required string RefreshToken { get; init; }
}
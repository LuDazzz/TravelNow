using System.ComponentModel.DataAnnotations;

namespace TravelNow.Application.Features.Auth.RefreshToken;

public sealed class RefreshTokenCommand
{
    [Required]
    public required string AccessToken { get; init; }

    [Required]
    public required string RefreshToken { get; init; }
}
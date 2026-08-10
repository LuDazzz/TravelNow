namespace TravelNow.Application.Features.Auth.RefreshToken;

public sealed record RefreshTokenResult(
    string AccessToken,
    string RefreshToken);
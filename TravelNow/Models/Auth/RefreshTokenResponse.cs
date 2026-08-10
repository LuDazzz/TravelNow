namespace TravelNow.Models.Auth;

public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken);
namespace TravelNow.Models.Auth;

public sealed record RegisterResponse(
    Guid UserId,
    string Email,
    string UserName,
    string AccessToken,
    string RefreshToken);
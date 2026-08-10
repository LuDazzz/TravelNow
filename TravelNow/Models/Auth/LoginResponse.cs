namespace TravelNow.Models.Auth;

public sealed record LoginResponse(
    Guid UserId,
    string Email,
    string UserName,
    IList<string> Roles,
    string AccessToken,
    string RefreshToken);
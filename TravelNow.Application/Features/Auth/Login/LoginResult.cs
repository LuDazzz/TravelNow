namespace TravelNow.Application.Features.Auth.Login;

public sealed record LoginResult(
    Guid UserId,
    string Email,
    string UserName,
    IList<string> Roles,
    string AccessToken,
    string RefreshToken);
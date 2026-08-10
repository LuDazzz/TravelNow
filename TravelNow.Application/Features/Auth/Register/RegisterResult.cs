namespace TravelNow.Application.Features.Auth.Register;

public sealed record RegisterResult(
    Guid UserId,
    string Email,
    string UserName,
    string AccessToken,
    string RefreshToken);
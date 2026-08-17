using System.Security.Claims;
using TravelNow.Domain.Entities.Identity;

namespace TravelNow.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, IList<string> roles);
    string GenerateRefreshToken();
    DateTimeOffset GetRefreshTokenExpiry();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken);
}
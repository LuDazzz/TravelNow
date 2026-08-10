using System.Security.Claims;
using TravelNow.Domain.Entities.Identity;

namespace TravelNow.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, IList<string> roles);
    string GenerateRefreshToken();
    DateTime DecodeRefreshToken(string refreshToken);
    ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken);
}
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TravelNow.Domain.Entities.Identity;

[Table("UserTokens")]
public class UserToken : IdentityUserToken<Guid>
{
    public virtual User User { get; set; }

    public string RefreshToken { get; set; }

    public DateTimeOffset RefreshTokenExpiration { get; set; }

    public bool RememberMe { get; set; }

    public string? DeviceInfo { get; set; }

    public string? LocationInfo { get; set; }
}
    
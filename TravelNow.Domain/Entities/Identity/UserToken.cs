using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TravelNow.Domain.Entities.Identity;

[Table("UserTokens")]
public class UserToken : IdentityUserToken<Guid>
{
}

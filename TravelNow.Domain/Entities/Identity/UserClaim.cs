using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TravelNow.Domain.Entities.Identity;

[Table("UserClaims")]
public class UserClaim : IdentityUserClaim<Guid>
{
}

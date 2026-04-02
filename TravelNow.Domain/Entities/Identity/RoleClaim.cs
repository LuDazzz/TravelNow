using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TravelNow.Domain.Entities.Identity;

[Table("RoleClaims")]
public class RoleClaim : IdentityRoleClaim<Guid>
{
}

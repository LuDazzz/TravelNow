using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TravelNow.Domain.Entities.Identity;

[Table("UserLogins")]
public class UserLogin : IdentityUserLogin<Guid>
{
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TravelNow.Domain.Entities.Base;
using TravelNow.Domain.Entities.Identity;

namespace TravelNow.Domain.Entities;

/// <summary>
/// Separate entity for storing refresh tokens. Does NOT use Identity UserToken (composite PK).
/// </summary>
[Table("RefreshTokens")]
public class RefreshTokenEntity : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }

    public virtual User User { get; set; } = null!;

    [Required]
    public required string Token { get; set; }

    public required DateTimeOffset Expires { get; set; }

    public bool IsUsed { get; set; }

    public bool IsRevoked { get; set; }

    public string? DeviceInfo { get; set; }
}
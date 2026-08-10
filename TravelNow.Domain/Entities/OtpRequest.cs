using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TravelNow.Domain.Entities.Base;
using TravelNow.Domain.Entities.Identity;

namespace TravelNow.Domain.Entities;

/// <summary>
/// Stores hashed OTP for password reset flow.
/// </summary>
[Table("OtpRequests")]
public class OtpRequest : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }

    public virtual User User { get; set; } = null!;

    [Required]
    public required string Email { get; set; }

    [Required]
    [MaxLength(6)]
    public required string OtpHash { get; set; }

    public required DateTimeOffset ExpiresAt { get; set; }

    public bool IsUsed { get; set; }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using TravelNow.Domain.Entities.Base;
using TravelNow.Domain.Enums;

namespace TravelNow.Domain.Entities.Identity;

[Table("Users")]
public class User : IdentityUser<Guid>, IAuditEntity, IIsDeletedEntity
{
    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [MaxLength(20)]
    public GenderEnum Gender { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = [];

    public virtual ICollection<UserToken> UserTokens { get; set; } = [];

    public virtual ICollection<Post> Posts { get; set; } = [];

    public virtual ICollection<Comment> Comments { get; set; } = [];

    public virtual ICollection<UserInteraction> UserInteractions { get; set; } = [];
}

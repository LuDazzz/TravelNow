using System.ComponentModel.DataAnnotations;

namespace TravelNow.Application.Features.Auth.ForgotPassword;

public sealed class ForgotPasswordCommand
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }
}
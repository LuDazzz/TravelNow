using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TravelNow.Application.Interfaces;

namespace TravelNow.Infrastructure.Features.Auth;

public sealed class SmtpMailService(IOptions<SmtpSettings> settings, ILogger<SmtpMailService> logger) : IMailService
{
    private readonly SmtpSettings _settings = settings.Value;

    public async Task SendAsync(string to, string subject, string body)
    {
        try
        {
            using var client = new SmtpClient(_settings.Host)
            {
                Port = _settings.Port,
                Credentials = new NetworkCredential(_settings.UserName, _settings.Password),
                EnableSsl = _settings.EnableSsl
            };

            var message = new MailMessage
            {
                From = new MailAddress(_settings.FromAddress, _settings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(to);

            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }
}

public sealed class SmtpSettings
{
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public bool EnableSsl { get; init; } = true;
    public string FromAddress { get; init; } = string.Empty;
    public string FromName { get; init; } = "TravelNow";
}
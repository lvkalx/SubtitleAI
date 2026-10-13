using System.Net;
using System.Net.Mail;

namespace SubtitleAI.Infrastructure.Auth;

public interface IEmailSender
{
    bool IsConfigured { get; }
    Task SendAsync(string recipient, string subject, string htmlBody, CancellationToken ct);
}

public sealed class SmtpEmailSender(EmailOptions options) : IEmailSender
{
    public bool IsConfigured => options.IsConfigured;

    public async Task SendAsync(string recipient, string subject, string htmlBody, CancellationToken ct)
    {
        if (!IsConfigured) throw new InvalidOperationException("Email delivery is not configured.");

        using var message = new MailMessage(options.From, recipient, subject, htmlBody) { IsBodyHtml = true };
        using var client = new SmtpClient(options.Host, options.Port)
        {
            EnableSsl = options.UseSsl,
            Credentials = string.IsNullOrWhiteSpace(options.UserName)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(options.UserName, options.Password)
        };
        await client.SendMailAsync(message, ct);
    }
}

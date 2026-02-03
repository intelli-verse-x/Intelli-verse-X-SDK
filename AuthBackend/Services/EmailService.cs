using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace AuthBackend.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _settings;

    public EmailService(IOptions<SmtpSettings> options)
    {
        _settings = options?.Value ?? new SmtpSettings();
    }

    public async Task SendOtpEmailAsync(string toEmail, string otp, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            throw new ArgumentException("Recipient email is required.", nameof(toEmail));
        if (string.IsNullOrWhiteSpace(otp))
            throw new ArgumentException("OTP is required.", nameof(otp));

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.UseSsl,
            Credentials = string.IsNullOrEmpty(_settings.Password)
                ? null
                : new NetworkCredential(_settings.UserName, _settings.Password)
        };

        var message = new MailMessage
        {
            From = new MailAddress(_settings.FromAddress ?? _settings.UserName ?? "noreply@localhost", _settings.FromDisplayName ?? "Auth Backend"),
            Subject = "Your verification code",
            Body = $"Your verification code is: {otp}. It is valid for 10 minutes.",
            IsBodyHtml = false
        };
        message.To.Add(toEmail.Trim());

        await client.SendMailAsync(message, cancellationToken);
    }
}

public class SmtpSettings
{
    public const string SectionName = "Smtp";
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? FromAddress { get; set; }
    public string? FromDisplayName { get; set; }
}

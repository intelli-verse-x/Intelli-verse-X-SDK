namespace AuthBackend.Services;

public interface IEmailService
{
    Task SendOtpEmailAsync(string toEmail, string otp, CancellationToken cancellationToken = default);
}

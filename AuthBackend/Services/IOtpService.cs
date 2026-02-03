namespace AuthBackend.Services;

public interface IOtpService
{
    string GenerateAndStore(string email, string? username = null);
    (bool Success, string? Username) Verify(string email, string otp);
}

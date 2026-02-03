using System.Collections.Concurrent;

namespace AuthBackend.Services;

public class OtpService : IOtpService
{
    private static readonly Random Rng = new Random();
    private static readonly TimeSpan OtpValidity = TimeSpan.FromMinutes(10);
    private readonly ConcurrentDictionary<string, (string Code, DateTime ExpiresAt, string? Username)> _store = new();

    public string GenerateAndStore(string email, string? username = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        string normalized = email.Trim().ToLowerInvariant();
        string code = Rng.Next(100000, 999999).ToString();
        _store[normalized] = (code, DateTime.UtcNow.Add(OtpValidity), username?.Trim());
        return code;
    }

    public (bool Success, string? Username) Verify(string email, string otp)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp))
            return (false, null);

        string normalized = email.Trim().ToLowerInvariant();
        if (!_store.TryGetValue(normalized, out var stored))
            return (false, null);

        if (DateTime.UtcNow > stored.ExpiresAt)
        {
            _store.TryRemove(normalized, out _);
            return (false, null);
        }

        if (stored.Code != otp.Trim())
            return (false, null);

        _store.TryRemove(normalized, out _);
        return (true, stored.Username);
    }
}

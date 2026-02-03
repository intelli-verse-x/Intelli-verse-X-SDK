namespace AuthBackend.Services;

public interface IGoogleAuthService
{
    Task<GooglePayload?> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken = default);
}

public class GooglePayload
{
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Sub { get; set; }
}

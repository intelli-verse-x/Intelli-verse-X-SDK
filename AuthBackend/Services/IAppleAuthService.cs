namespace AuthBackend.Services;

public interface IAppleAuthService
{
    Task<ApplePayload?> ValidateIdentityTokenAsync(string identityToken, CancellationToken cancellationToken = default);
}

public class ApplePayload
{
    public string? Email { get; set; }
    public string? Sub { get; set; }
    public string? Name { get; set; }
}

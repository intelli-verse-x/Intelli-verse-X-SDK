using Google.Apis.Auth;

namespace AuthBackend.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IConfiguration _configuration;

    public GoogleAuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<GooglePayload?> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
            return null;

        try
        {
            var clientId = _configuration["Google:ClientId"];
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = string.IsNullOrEmpty(clientId) ? null : new[] { clientId }
            });

            return new GooglePayload
            {
                Email = payload.Email ?? string.Empty,
                Name = payload.Name,
                Sub = payload.Subject
            };
        }
        catch
        {
            return null;
        }
    }
}

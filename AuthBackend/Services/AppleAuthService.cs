using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace AuthBackend.Services;

public class AppleAuthService : IAppleAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IConfigurationManager<OpenIdConnectConfiguration> _configManager;

    public AppleAuthService(IConfiguration configuration)
    {
        _configuration = configuration;
        var appleKeysUrl = "https://appleid.apple.com/auth/keys";
        _configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            appleKeysUrl,
            new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever());
    }

    public async Task<ApplePayload?> ValidateIdentityTokenAsync(string identityToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identityToken))
            return null;

        try
        {
            var config = await _configManager.GetConfigurationAsync(cancellationToken);
            var clientId = _configuration["Apple:ClientId"];

            var validationParameters = new TokenValidationParameters
            {
                ValidIssuer = "https://appleid.apple.com",
                ValidAudience = clientId,
                IssuerSigningKeys = config.SigningKeys,
                ValidateIssuer = true,
                ValidateAudience = !string.IsNullOrEmpty(clientId),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2)
            };

            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(identityToken, validationParameters, out _);
            var jwt = handler.ReadJwtToken(identityToken);

            string? email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            string? sub = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            string? name = null;
            if (jwt.Payload.TryGetValue("name", out var nameObj) && nameObj != null)
            {
                var nameJson = nameObj is string s ? s : JsonSerializer.Serialize(nameObj);
                try
                {
                    var nameDoc = JsonDocument.Parse(nameJson);
                    name = nameDoc.RootElement.TryGetProperty("firstName", out var fn) ? fn.GetString() : null;
                }
                catch
                {
                    name = nameJson;
                }
            }

            return new ApplePayload { Email = email, Sub = sub, Name = name };
        }
        catch
        {
            return null;
        }
    }
}

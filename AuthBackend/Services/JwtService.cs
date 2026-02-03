using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthBackend.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthBackend.Services;

public class JwtService : IJwtService
{
    private readonly JwtSettings _settings;
    private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromDays(7);
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    public JwtService(IOptions<JwtSettings> options)
    {
        _settings = options?.Value ?? new JwtSettings();
    }

    public (string AccessToken, string RefreshToken, int ExpiresInSeconds) GenerateTokens(UserDto user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.Add(AccessTokenLifetime);
        var expiresInSeconds = (int)AccessTokenLifetime.TotalSeconds;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim("idp_username", user.IdpUsername ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role ?? "user"),
            new Claim("is_guest", user.IsGuest ? "true" : "false"),
            new Claim("login_type", user.LoginType ?? "email")
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return (accessToken, refreshToken, expiresInSeconds);
    }
}

public class JwtSettings
{
    public const string SectionName = "Jwt";
    public string SecretKey { get; set; } = "YourSuperSecretKeyForJwtTokensMustBeAtLeast32Characters";
}

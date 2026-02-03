using AuthBackend.Models;

namespace AuthBackend.Services;

public interface IJwtService
{
    (string AccessToken, string RefreshToken, int ExpiresInSeconds) GenerateTokens(UserDto user);
}

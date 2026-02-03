namespace AuthBackend.Models;

public class LoginResponse
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public LoginResponseData? Data { get; set; }
}

public class LoginResponseData
{
    public UserDto? User { get; set; }
    public string Token { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}

namespace AuthBackend.Models;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string IdpUsername { get; set; } = string.Empty;
    public string Role { get; set; } = "user";
    public bool IsGuest { get; set; }
    public string LoginType { get; set; } = "email";
    
    /// <summary>
    /// Indicates if the user account has been verified via OTP.
    /// OTP verification activates the account and allows login.
    /// </summary>
    public bool IsVerified { get; set; }
    
    /// <summary>
    /// Hashed password for email/password login.
    /// Stored during registration, used during login.
    /// </summary>
    public string? PasswordHash { get; set; }
}

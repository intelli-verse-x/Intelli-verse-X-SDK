using AuthBackend.Models;

namespace AuthBackend.Services;

public interface IUserService
{
    UserDto? GetByEmail(string email);
    UserDto? GetById(string id);
    UserDto CreateEmailUser(string email, string username, string? passwordHash = null);
    UserDto CreateGuestUser();
    UserDto GetOrCreateFromGoogle(string email, string? name, string? googleId);
    UserDto GetOrCreateFromApple(string email, string? name, string? appleSub);
    
    /// <summary>
    /// Mark user as verified (after OTP verification).
    /// OTP verification activates the account and allows login.
    /// </summary>
    void MarkAsVerified(string email);
    
    /// <summary>
    /// Verify password for email/password login.
    /// </summary>
    bool VerifyPassword(string email, string password);
}

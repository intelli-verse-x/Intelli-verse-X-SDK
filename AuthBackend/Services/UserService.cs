using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using AuthBackend.Models;

namespace AuthBackend.Services;

/// <summary>
/// In-memory user store shared by all authentication endpoints.
/// Both /auth/verify-otp and /auth/login use this same store.
/// OTP verification activates accounts, login requires verified accounts.
/// </summary>
public class UserService : IUserService
{
    private readonly ConcurrentDictionary<string, UserDto> _byId = new();
    private readonly ConcurrentDictionary<string, string> _emailToId = new(StringComparer.OrdinalIgnoreCase);
    private int _idCounter;

    public UserDto? GetByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;
        var key = email.Trim().ToLowerInvariant();
        return _emailToId.TryGetValue(key, out var id) && _byId.TryGetValue(id, out var user) ? user : null;
    }

    public UserDto? GetById(string id)
    {
        return string.IsNullOrWhiteSpace(id) ? null : _byId.TryGetValue(id, out var user) ? user : null;
    }

    /// <summary>
    /// Create email user with optional password hash.
    /// If user exists, updates password hash if provided and user is not verified.
    /// </summary>
    public UserDto CreateEmailUser(string email, string username, string? passwordHash = null)
    {
        var existing = GetByEmail(email);
        if (existing != null)
        {
            // Update password hash if provided and user is not verified yet
            if (!string.IsNullOrEmpty(passwordHash) && !existing.IsVerified)
            {
                existing.PasswordHash = passwordHash;
            }
            return existing;
        }

        var id = "usr_" + Interlocked.Increment(ref _idCounter);
        var user = new UserDto
        {
            Id = id,
            Email = email.Trim(),
            UserName = username.Trim(),
            IdpUsername = id,
            Role = "user",
            IsGuest = false,
            LoginType = "email",
            IsVerified = false, // Will be set to true after OTP verification
            PasswordHash = passwordHash
        };

        _byId[id] = user;
        _emailToId[user.Email.ToLowerInvariant()] = id;
        return user;
    }
    
    /// <summary>
    /// Mark user as verified after OTP verification.
    /// OTP verification activates the account and allows login.
    /// 
    /// CRITICAL: This updates the user in the shared store.
    /// Both /auth/verify-otp and /auth/login use this same store.
    /// </summary>
    public void MarkAsVerified(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return;

        var key = email.Trim().ToLowerInvariant();
        if (!_emailToId.TryGetValue(key, out var id))
        {
            Console.WriteLine($"WARNING: MarkAsVerified called for non-existent user: {email}");
            return;
        }

        if (!_byId.TryGetValue(id, out var user))
        {
            Console.WriteLine($"WARNING: MarkAsVerified - User ID {id} not found in store for {email}");
            return;
        }

        // Update the user in the shared store
        // This ensures /auth/login will see IsVerified = true
        user.IsVerified = true;
        Console.WriteLine($"User marked as verified: {email} (ID: {id})");
    }
    
    /// <summary>
    /// Verify password for email/password login.
    /// </summary>
    public bool VerifyPassword(string email, string password)
    {
        var user = GetByEmail(email);
        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            return false;
        
        // Simple hash comparison (in production, use BCrypt or similar)
        var inputHash = HashPassword(password);
        return inputHash == user.PasswordHash;
    }
    
    /// <summary>
    /// Hash password using SHA256 (simple implementation).
    /// In production, use BCrypt, Argon2, or similar.
    /// </summary>
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public UserDto CreateGuestUser()
    {
        var id = "guest_" + Guid.NewGuid().ToString("N")[..12];
        var user = new UserDto
        {
            Id = id,
            Email = $"guest-{id}@local",
            UserName = "Guest",
            IdpUsername = id,
            Role = "user",
            IsGuest = true,
            LoginType = "guest"
        };

        _byId[id] = user;
        return user;
    }

    public UserDto GetOrCreateFromGoogle(string email, string? name, string? googleId)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required for Google login.", nameof(email));

        var existing = GetByEmail(email);
        if (existing != null)
            return existing;

        var id = "usr_" + Interlocked.Increment(ref _idCounter);
        var user = new UserDto
        {
            Id = id,
            Email = email.Trim(),
            UserName = name?.Trim() ?? email.Split('@')[0],
            IdpUsername = googleId ?? id,
            Role = "user",
            IsGuest = false,
            LoginType = "google"
        };

        _byId[id] = user;
        _emailToId[user.Email.ToLowerInvariant()] = id;
        return user;
    }

    public UserDto GetOrCreateFromApple(string email, string? name, string? appleSub)
    {
        var sub = appleSub ?? Guid.NewGuid().ToString("N")[..16];
        var id = "usr_apple_" + sub;

        if (_byId.TryGetValue(id, out var existing))
            return existing;

        var user = new UserDto
        {
            Id = id,
            Email = string.IsNullOrWhiteSpace(email) ? $"apple_{sub}@privaterelay.appleid.com" : email.Trim(),
            UserName = name?.Trim() ?? "Apple User",
            IdpUsername = sub,
            Role = "user",
            IsGuest = false,
            LoginType = "apple"
        };

        _byId[id] = user;
        if (!string.IsNullOrWhiteSpace(user.Email))
            _emailToId[user.Email.ToLowerInvariant()] = id;
        return user;
    }
}

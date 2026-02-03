using AuthBackend.Models;
using AuthBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthBackend.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly IJwtService _jwtService;
    private readonly IUserService _userService;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IAppleAuthService _appleAuthService;

    public AuthController(
        IOtpService otpService,
        IEmailService emailService,
        IJwtService jwtService,
        IUserService userService,
        IGoogleAuthService googleAuthService,
        IAppleAuthService appleAuthService)
    {
        _otpService = otpService;
        _emailService = emailService;
        _jwtService = jwtService;
        _userService = userService;
        _googleAuthService = googleAuthService;
        _appleAuthService = appleAuthService;
    }

    /// <summary>
    /// Register: generate OTP and send via email.
    /// Stores password hash for later login.
    /// User account is NOT activated until OTP is verified.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new LoginResponse { Status = false, Message = "Email is required." });

        // Store password hash during registration (for later login)
        // User is created but NOT verified until OTP is verified
        // CreateEmailUser will update password hash if user exists and is not verified
        var passwordHash = HashPassword(request.Password);
        _userService.CreateEmailUser(request.Email.Trim(), request.Username, passwordHash);

        // Generate and send OTP
        var otp = _otpService.GenerateAndStore(request.Email.Trim(), request.Username);
        try
        {
            await _emailService.SendOtpEmailAsync(request.Email.Trim(), otp, cancellationToken);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new LoginResponse { Status = false, Message = "Failed to send email: " + ex.Message });
        }

        return Ok(new LoginResponse { Status = true, Message = "OTP sent to your email." });
    }
    
    /// <summary>
    /// Simple password hashing (SHA256).
    /// In production, use BCrypt, Argon2, or similar.
    /// </summary>
    private static string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Verify OTP and activate account.
    /// OTP verification activates the account and logs user in automatically.
    /// After this, user can login with email + password.
    /// 
    /// CRITICAL: User MUST exist from registration before OTP verification.
    /// Both /auth/verify-otp and /auth/login use the SAME shared UserService store.
    /// </summary>
    [HttpPost("verify-otp")]
    public IActionResult VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Otp))
            return BadRequest(new LoginResponse { Status = false, Message = "Email and OTP are required." });

        var email = request.Email.Trim();
        var otp = request.Otp.Trim();

        // Step 1: Verify OTP (NEVER log OTP value)
        var (verified, pendingUsername) = _otpService.Verify(email, otp);
        if (!verified)
            return Unauthorized(new LoginResponse { Status = false, Message = "Invalid or expired OTP." });

        // Step 2: Get user from the SAME shared store used by /auth/login
        // User should exist from registration (with password hash)
        var user = _userService.GetByEmail(email);
        if (user == null)
        {
            // User should exist from registration - this is unexpected
            // Create user as fallback, but log warning
            Console.WriteLine($"WARNING: User not found during OTP verification for {email}. Creating user without password hash.");
            user = _userService.CreateEmailUser(email, pendingUsername ?? email.Split('@')[0]);
        }

        // Step 3: OTP verification activates the account - mark as verified
        // This MUST persist in the shared store so /auth/login can find it
        _userService.MarkAsVerified(email);
        
        // Step 4: Refresh user reference to ensure we have the latest state
        // (MarkAsVerified updates the stored user, so get fresh reference)
        user = _userService.GetByEmail(email);
        if (user == null)
        {
            Console.WriteLine($"ERROR: User disappeared after MarkAsVerified for {email}");
            return StatusCode(500, new LoginResponse { Status = false, Message = "Internal error: User not found after verification." });
        }

        // Step 5: Verify user is marked as verified
        if (!user.IsVerified)
        {
            Console.WriteLine($"ERROR: User verification flag not set for {email}");
            return StatusCode(500, new LoginResponse { Status = false, Message = "Internal error: Verification failed." });
        }

        // Step 6: Generate JWT tokens ONLY after user is saved and verified
        var (accessToken, refreshToken, expiresIn) = _jwtService.GenerateTokens(user);
        var data = new LoginResponseData
        {
            User = user,
            Token = accessToken,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = expiresIn
        };

        // Log success (without OTP value)
        Console.WriteLine($"OTP verification succeeded for {email} - User ID: {user.Id}, Verified: {user.IsVerified}");

        return Ok(new LoginResponse { Status = true, Message = "OK", Data = data });
    }

    /// <summary>
    /// Google login: verify ID token and issue JWT tokens.
    /// </summary>
    [HttpPost("google")]
    public async Task<IActionResult> Google([FromBody] SocialLoginRequest request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.IdToken))
            return BadRequest(new LoginResponse { Status = false, Message = "IdToken is required." });

        var payload = await _googleAuthService.ValidateIdTokenAsync(request.IdToken.Trim(), cancellationToken);
        if (payload == null)
            return Unauthorized(new LoginResponse { Status = false, Message = "Invalid Google token." });

        var user = _userService.GetOrCreateFromGoogle(payload.Email, payload.Name, payload.Sub);
        var (accessToken, refreshToken, expiresIn) = _jwtService.GenerateTokens(user);
        var data = new LoginResponseData
        {
            User = user,
            Token = accessToken,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = expiresIn
        };

        return Ok(new LoginResponse { Status = true, Message = "OK", Data = data });
    }

    /// <summary>
    /// Apple login: verify identity token and issue JWT tokens.
    /// </summary>
    [HttpPost("apple")]
    public async Task<IActionResult> Apple([FromBody] SocialLoginRequest request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.IdToken))
            return BadRequest(new LoginResponse { Status = false, Message = "IdToken is required." });

        var payload = await _appleAuthService.ValidateIdentityTokenAsync(request.IdToken.Trim(), cancellationToken);
        if (payload == null)
            return Unauthorized(new LoginResponse { Status = false, Message = "Invalid Apple token." });

        var user = _userService.GetOrCreateFromApple(payload.Email ?? "", payload.Name, payload.Sub);
        var (accessToken, refreshToken, expiresIn) = _jwtService.GenerateTokens(user);
        var data = new LoginResponseData
        {
            User = user,
            Token = accessToken,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = expiresIn
        };

        return Ok(new LoginResponse { Status = true, Message = "OK", Data = data });
    }

    /// <summary>
    /// Email/password login.
    /// Requires account to be verified via OTP first.
    /// Uses the SAME user store as /auth/verify-otp.
    /// 
    /// CRITICAL: Both endpoints use the SAME IUserService singleton.
    /// If user was verified via OTP, login will find the same user.
    /// </summary>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new LoginResponse { Status = false, Message = "Email and password are required." });

        var email = request.Email.Trim();
        var password = request.Password;

        // Step 1: Look up user from the SAME shared store used by /auth/verify-otp
        // This is the SAME UserService instance (singleton) used in VerifyOtp
        var user = _userService.GetByEmail(email);
        
        if (user == null)
        {
            // User not found in shared store
            // This means either:
            // - User never registered
            // - User was created in a different store (should not happen with singleton)
            Console.WriteLine($"Login failed: User not found in shared store for {email}");
            return Unauthorized(new LoginResponse { Status = false, Message = "Account not found. Please register first." });
        }

        // Step 2: Check if account is verified (OTP verification required)
        // This flag was set by /auth/verify-otp in the SAME shared store
        if (!user.IsVerified)
        {
            Console.WriteLine($"Login failed: Account not verified for {email} (IsVerified: {user.IsVerified})");
            return Unauthorized(new LoginResponse { Status = false, Message = "Account not verified. Please verify OTP first." });
        }

        // Step 3: Verify password
        // Password hash was stored during registration in the SAME shared store
        if (!_userService.VerifyPassword(email, password))
        {
            Console.WriteLine($"Login failed: Invalid password for {email}");
            return Unauthorized(new LoginResponse { Status = false, Message = "Invalid email or password." });
        }

        // Step 4: Generate JWT tokens for verified user
        var (accessToken, refreshToken, expiresIn) = _jwtService.GenerateTokens(user);
        var data = new LoginResponseData
        {
            User = user,
            Token = accessToken,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = expiresIn
        };

        Console.WriteLine($"Login succeeded for {email} - User ID: {user.Id}, Verified: {user.IsVerified}");
        return Ok(new LoginResponse { Status = true, Message = "OK", Data = data });
    }

    /// <summary>
    /// Guest login: create guest user and issue JWT tokens.
    /// </summary>
    [HttpPost("guest")]
    public IActionResult Guest()
    {
        var user = _userService.CreateGuestUser();
        var (accessToken, refreshToken, expiresIn) = _jwtService.GenerateTokens(user);
        var data = new LoginResponseData
        {
            User = user,
            Token = accessToken,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = expiresIn
        };

        return Ok(new LoginResponse { Status = true, Message = "OK", Data = data });
    }
}

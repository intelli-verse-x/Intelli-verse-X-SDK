using System.ComponentModel.DataAnnotations;

namespace AuthBackend.Models;

public class VerifyOtpRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(6, MinimumLength = 6)]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be 6 digits")]
    public string Otp { get; set; } = string.Empty;
}

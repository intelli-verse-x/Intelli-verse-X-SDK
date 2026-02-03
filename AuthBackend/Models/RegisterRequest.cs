using System.ComponentModel.DataAnnotations;

namespace AuthBackend.Models;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public string Username { get; set; } = string.Empty;
}

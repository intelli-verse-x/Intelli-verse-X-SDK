using System.ComponentModel.DataAnnotations;

namespace AuthBackend.Models;

public class SocialLoginRequest
{
    [Required]
    public string IdToken { get; set; } = string.Empty;
}

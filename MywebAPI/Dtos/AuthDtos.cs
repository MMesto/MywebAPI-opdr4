using System.ComponentModel.DataAnnotations;

namespace MywebAPI.Dtos;

public class RegisterDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Wachtwoord { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Wachtwoord { get; set; } = string.Empty;    
}

public class GebruikerDto
{
    public string Email { get; set; } = string.Empty;
    public List<string> Rollen { get; set; } = [];
}
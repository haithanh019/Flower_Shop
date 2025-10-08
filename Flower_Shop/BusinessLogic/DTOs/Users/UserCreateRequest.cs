using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Users;

public class UserCreateRequest
{
    [Required, EmailAddress]
    public required string Email { get; set; }

    [Required, MinLength(6)]
    public required string Password { get; set; }

    [Required]
    public required string FullName { get; set; }

    public string? PhoneNumber { get; set; }
    public string? Role { get; set; } = "Customer";
}

namespace BusinessLogic.DTOs.Users;

public class UserDto
{
    public Guid UserId { get; set; }
    public required string Email { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public required string Role { get; set; }
    public DateTime CreatedAt { get; set; }
}

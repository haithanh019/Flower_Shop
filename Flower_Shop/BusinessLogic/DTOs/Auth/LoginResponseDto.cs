namespace BusinessLogic.DTOs.Auth;

public class LoginResponseDto
{
    public required Guid UserId { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
    public required string AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
}

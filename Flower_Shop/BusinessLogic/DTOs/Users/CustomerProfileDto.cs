namespace BusinessLogic.DTOs.Users
{
    public class CustomerProfileDto
    {
        public Guid UserId { get; set; }
        public required string Email { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

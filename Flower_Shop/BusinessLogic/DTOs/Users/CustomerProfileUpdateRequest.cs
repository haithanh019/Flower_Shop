using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Users
{
    public class CustomerProfileUpdateRequest
    {
        [Required, MaxLength(100)]
        public string? FullName { get; set; }

        [Phone, MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }
    }
}

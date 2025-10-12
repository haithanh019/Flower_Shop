using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Users
{
    public class CustomerProfileUpdateRequest
    {
        [MaxLength(100)]
        public string? FullName { get; set; }

        [Phone, MaxLength(20)]
        public string? PhoneNumber { get; set; }
    }
}

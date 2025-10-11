using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Address
{
    public class AddressUpdateRequest
    {
        [Required]
        public Guid AddressId { get; set; }

        [Required, MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string District { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Ward { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string Detail { get; set; } = string.Empty;
    }
}

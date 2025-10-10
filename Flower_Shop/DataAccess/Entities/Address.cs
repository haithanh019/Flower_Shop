using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities
{
    public class Address
    {
        [Key]
        public Guid AddressId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }
        public User? User { get; set; }

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

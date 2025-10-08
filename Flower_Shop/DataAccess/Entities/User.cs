using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(100)]
        public string FullName { get; set; } = "";

        [Required, MaxLength(150)]
        public string Email { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";

        public UserRole Role { get; set; } = UserRole.Customer;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public Cart? Cart { get; set; }
    }

    public enum UserRole
    {
        [Display(Name = "Customer")]
        Customer = 0,

        [Display(Name = "Admin")]
        Admin = 1,
    }
}

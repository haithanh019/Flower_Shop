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

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public Cart? Cart { get; set; }
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }

    public enum UserRole
    {
        [Display(Name = "Khách hàng")]
        Customer = 0,

        [Display(Name = "Quản trị viên")]
        Admin = 1,
    }
}

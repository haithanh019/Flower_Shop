using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities
{
    public class Cart
    {
        [Key]
        public Guid CartId { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid? UserId { get; set; }
        public User? User { get; set; }

        [MaxLength(100)]
        public string? SessionId { get; set; }
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Entities
{
    public class Product
    {
        [Key]
        public Guid ProductId { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(150)]
        public string Name { get; set; } = "";

        [MaxLength(150)]
        public string? Slug { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Precision(18, 2)]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}

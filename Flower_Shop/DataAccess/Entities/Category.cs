using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities
{
    public class Category
    {
        [Key]
        public Guid CategoryId { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(100)]
        public string Name { get; set; } = "";

        [MaxLength(150)]
        public string? Slug { get; set; }

        [MaxLength(300)]
        public string? Description { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}

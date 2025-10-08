using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities
{
    public class ProductImage
    {
        [Key]
        public Guid ProductImageId { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string Url { get; set; } = "";

        [MaxLength(100)]
        public string? PublicId { get; set; }

        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
    }
}

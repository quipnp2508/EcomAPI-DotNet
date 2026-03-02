using System.ComponentModel.DataAnnotations;

namespace EComAPI.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Required]
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsDeleted { get; set; } = false;
        public Guid CategoryId { get; set; }
        public Category Category { get; set; }
    }
}

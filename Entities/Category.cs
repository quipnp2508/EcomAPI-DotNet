using System.ComponentModel.DataAnnotations;

namespace EComAPI.Entities
{
    public class Category
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        public bool IsDeleted { get; set; } = false;
        public List<Product> Products { get; set; }
    }
}

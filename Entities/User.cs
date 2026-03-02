using System.ComponentModel.DataAnnotations;

namespace EComAPI.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "User";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

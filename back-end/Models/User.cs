using System.ComponentModel.DataAnnotations;

namespace RecipeManager.Models
{
    public class User
    {
        public int UserId { get; private set; }

        [Required]
        [StringLength(50)]
        public required string Username { get; set; }

        [Required]
        [StringLength(100)]
        public required string PasswordHash { get; set; }
    }
}
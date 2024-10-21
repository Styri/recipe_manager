using System.ComponentModel.DataAnnotations;

namespace RecipeManager.Models
{
    public class UserRegistrationDto
    {
        [Required]
        [StringLength(50)]
        public required string Username { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public required string Password { get; set; }
    }
}
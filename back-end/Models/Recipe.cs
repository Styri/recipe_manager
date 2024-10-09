using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RecipeManager.Models
{
    public class Recipe
    {
        [JsonPropertyName("recipeId")]
        public int RecipeId { get; private set; }

        [JsonPropertyName("name")]
        [Required]
        [StringLength(250)]
        public required string Name { get; set; }

        [JsonPropertyName("description")]
        [Required]
        [StringLength(500)]
        public required string Description { get; set; }

        [JsonPropertyName("category")]
        public required string Category { get; set; } ="Other";

        [JsonPropertyName("favorite")]
        public bool Favorite { get; set; }

        public static readonly string[] ValidCategories = ["Breakfast", "Brunch", "Lunch", "Dinner", "Supper", "Other"];
    }
}
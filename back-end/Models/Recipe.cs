using System.Text.Json.Serialization;

namespace RecipeManager.Models
{
    public class Recipe
    {
        [JsonPropertyName("recipeId")]
        public int RecipeId { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("description")]
        public required string Description { get; set; }

        [JsonPropertyName("category")]
        public required string Category { get; set; }

        [JsonPropertyName("favorite")]
        public bool Favorite { get; set; }

        public static readonly string[] ValidCategories = ["Breakfast", "Brunch", "Lunch", "Dinner", "Supper", "Other"];
    }
}
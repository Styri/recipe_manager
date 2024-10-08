using System.Text.Json.Serialization;

namespace RecipeManager.Models
{
    public class Recipe
    {
        [JsonPropertyName("recipeId")]
        public int RecipeId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("favorite")]
        public bool Favorite { get; set; }

        public static readonly string[] ValidCategories = new string[]
        { "Breakfast", "Brunch", "Lunch", "Dinner", "Supper", "Other" };
    }
}
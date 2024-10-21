using Dapper;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using RecipeManager.Models;
// using System.Collections.Generic;
// using Microsoft.AspNetCore.Authorization;


[ApiController]
[Route("recipes")]
public class RecipeController : ControllerBase
{
    private readonly IDbConnection _dbConnection;
    private readonly ILogger<RecipeController> _logger;

    public RecipeController(IDbConnection dbConnection, ILogger<RecipeController> logger)
    {
        _dbConnection = dbConnection;
        _logger = logger;
    }

    private ObjectResult HandleException(Exception ex, string errorMessage, string userMessage)
    {
        _logger.LogError(ex, errorMessage);
        return StatusCode(500, userMessage);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRecipes(int page = 1, int limit = 8)
    {
        try
        {
            var countQuery = "SELECT COUNT(*) FROM recipes";
            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countQuery);

            int totalPages = (int)Math.Ceiling((double)totalCount / limit);

            if (page < 1 || (totalCount > 0 && page > totalPages))
            {
                return BadRequest($"Invalid page number. Valid range: 1 to {totalPages}");
            }

            int offset = (page - 1) * limit;
            var query = @"
            SELECT recipe_id AS RecipeId, name AS Name, description AS Description, category AS Category, favorite AS Favorite 
            FROM recipes 
            ORDER BY recipe_id 
            LIMIT @Limit OFFSET @Offset";

            var recipes = await _dbConnection.QueryAsync<Recipe>(query, new { Limit = limit, Offset = offset });

            return Ok(new
            {
                totalCount,
                recipes,
                page,
                limit,
                totalPages
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error fetching recipes", "Unable to retrieve recipes. Please try again later.");
        }
    }

    [HttpGet("favorites")]
    public async Task<IActionResult> GetFavoriteRecipes(int page = 1, int limit = 8)
    {
        try
        {
            var favoriteCountQuery = "SELECT COUNT(*) FROM recipes WHERE favorite = 1";
            var totalFavoriteCount = await _dbConnection.ExecuteScalarAsync<int>(favoriteCountQuery);

            int totalPages = (int)Math.Ceiling((double)totalFavoriteCount / limit);

            if (page < 1 || (totalFavoriteCount > 0 && page > totalPages))
            {
                return BadRequest($"Invalid page number. Valid range: 1 to {totalPages}");
            }

            int offset = (page - 1) * limit;

            var query = @"SELECT recipe_id AS RecipeId, name AS Name, description AS Description,
                        category AS Category, favorite AS Favorite FROM recipes
                        WHERE favorite = 1 ORDER BY recipe_id LIMIT @Limit OFFSET @Offset";
            var favoriteRecipes = await _dbConnection.QueryAsync<Recipe>(query, new { Limit = limit, Offset = offset });
            return Ok(new
            {
                totalFavoriteCount,
                favoriteRecipes,
                page,
                limit,
                totalPages
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error fetching favorite recipes", "Unable to retrieve favorite recipes. Please try again later.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRecipe(int id)
    {
        try
        {
            var query = "DELETE FROM recipes WHERE recipe_id = @Id";
            var rowsAffected = await _dbConnection.ExecuteAsync(query, new { Id = id });
            if (rowsAffected > 0)
            {
                return Ok("Recipe deleted successfully");
            }
            else
            {
                return NotFound($"No recipe found with ID {id}");
            }
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error deleting recipe with ID {id}", $"Unable to delete recipe with ID {id}. Please try again later.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateRecipe([FromBody] Recipe newRecipe)
    {
        try
        {
            var query = @"
                INSERT INTO recipes (name, description, category, favorite) 
                VALUES (@Name, @Description, @Category, @Favorite);
                SELECT last_insert_rowid();";

            var recipeId = await _dbConnection.ExecuteScalarAsync<int>(query, newRecipe);

            var createdRecipe = await _dbConnection.QuerySingleOrDefaultAsync<Recipe>(
                "SELECT recipe_id AS RecipeId, name AS Name, description AS Description, category AS Category, favorite AS Favorite FROM recipes WHERE recipe_id = @Id",
                new { Id = recipeId });

            if (createdRecipe != null)
            {
                return CreatedAtAction(nameof(GetRecipe), new { id = createdRecipe.RecipeId }, createdRecipe);
            }

            return StatusCode(500, "Unable to create recipe. Please try again.");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error creating recipe", "Unable to create recipe. Please check your input and try again.");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRecipe(int id)
    {
        try
        {
            var query = @"SELECT recipe_id AS RecipeId, name AS Name, description AS Description, category AS Category,
                        favorite AS Favorite FROM recipes WHERE recipe_id = @Id";
            var recipe = await _dbConnection.QuerySingleOrDefaultAsync<Recipe>(query, new { Id = id });

            if (recipe == null)
            {
                return NotFound($"Recipe with ID {id} not found.");
            }

            return Ok(recipe);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error retrieving recipe with ID {id}", $"Unable to retrieve recipe with ID {id}. Please try again later.");
        }
    }

    [HttpGet("sort")]
    public async Task<IActionResult> SortRecipesByCategory(int page = 1, int limit = 8)
    {
        try
        {
            var countQuery = "SELECT COUNT(*) FROM recipes";
            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countQuery);
            int totalPages = (int)Math.Ceiling((double)totalCount / limit);

            if (page < 1 || (totalCount > 0 && page > totalPages))
            {
                return BadRequest($"Invalid page number. Valid range: 1 to {totalPages}");
            }

            int offset = (page - 1) * limit;
            var query = @"SELECT recipe_id AS RecipeId, name AS Name, description AS Description,
                    category AS Category, favorite AS Favorite FROM recipes
                    ORDER BY category, recipe_id
                    LIMIT @Limit OFFSET @Offset";

            var recipes = await _dbConnection.QueryAsync<Recipe>(query, new { Limit = limit, Offset = offset });

            return Ok(new
            {
                totalCount,
                recipes,
                page,
                limit,
                totalPages
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error sorting recipes by category", "Unable to sort recipes by category. Please try again later.");
        }
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GenerateRecipeReport()
    {
        try
        {
            var totalRecipesQuery = "SELECT COUNT(*) FROM recipes";
            var totalRecipes = await _dbConnection.ExecuteScalarAsync<int>(totalRecipesQuery);

            var favoriteRecipesQuery = "SELECT COUNT(*) FROM recipes WHERE favorite = 1";
            var favoriteRecipes = await _dbConnection.ExecuteScalarAsync<int>(favoriteRecipesQuery);

            var nonFavoriteRecipesQuery = "SELECT COUNT(*) FROM recipes WHERE favorite = 0";
            var nonFavoriteRecipes = await _dbConnection.ExecuteScalarAsync<int>(nonFavoriteRecipesQuery);

            var recipeStats = new
            {
                totalRecipes,
                favoriteRecipes,
                nonFavoriteRecipes
            };

            return Ok(recipeStats);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error generating recipe stats", "Unable to generate recipe statistics. Please try again later.");
        }
    }

    [HttpPatch("{id}/favorite")]
    public async Task<IActionResult> ToggleFavoriteStatus(int id)
    {
        try
        {
            var query = "UPDATE recipes SET favorite = ((favorite | 1) - (favorite & 1)) WHERE recipe_id = @Id";
            await _dbConnection.ExecuteAsync(query, new { Id = id });

            var newStatusQuery = "SELECT favorite FROM recipes WHERE recipe_id = @Id";
            var newFavoriteStatus = await _dbConnection.ExecuteScalarAsync<bool>(newStatusQuery, new { Id = id });

            return Ok(newFavoriteStatus);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error toggling favorite status for recipe with ID {id}", $"Unable to update favorite status for recipe with ID {id}. Please try again later.");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRecipeDescription(int id, [FromForm] string description)
    {
        try
        {
            var query = "UPDATE recipes SET description = @Description WHERE recipe_id = @Id";
            await _dbConnection.ExecuteAsync(query, new { Id = id, Description = description });

            var updatedRecipe = await _dbConnection.QuerySingleOrDefaultAsync<Recipe>(
                "SELECT recipe_id AS RecipeId, name AS Name, description AS Description, category AS Category, favorite AS Favorite FROM recipes WHERE recipe_id = @Id",
                new { Id = id });

            if (updatedRecipe == null)
            {
                return NotFound($"Recipe with ID {id} not found.");
            }

            return Ok(updatedRecipe);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"Error updating description for recipe with ID {id}", $"Unable to update description for recipe with ID {id}. Please try again later.");
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchRecipes([FromQuery] string searchTerm = "", int page = 1, int limit = 8)
    {
        try
        {
            var countQuery = @"
            SELECT COUNT(*) 
            FROM recipes 
            WHERE lower(name) LIKE lower(@SearchTerm) 
               OR lower(description) LIKE lower(@SearchTerm) 
               OR lower(category) LIKE lower(@SearchTerm)";

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countQuery, new { SearchTerm = $"%{searchTerm}%" });
            int totalPages = (int)Math.Ceiling((double)totalCount / limit);

            if (page < 1 || (totalCount > 0 && page > totalPages))
            {
                return BadRequest($"Invalid page number. Valid range: 1 to {totalPages}");
            }

            int offset = (page - 1) * limit;
            var query = @"
            SELECT recipe_id AS RecipeId, name AS Name, description AS Description, category AS Category, favorite AS Favorite 
            FROM recipes 
            WHERE lower(name) LIKE lower(@SearchTerm) 
               OR lower(description) LIKE lower(@SearchTerm) 
               OR lower(category) LIKE lower(@SearchTerm)
            ORDER BY recipe_id
            LIMIT @Limit OFFSET @Offset";

            var parameters = new
            {
                SearchTerm = $"%{searchTerm}%",
                Limit = limit,
                Offset = offset
            };

            var recipes = await _dbConnection.QueryAsync<Recipe>(query, parameters);

            return Ok(new
            {
                totalCount,
                recipes,
                page,
                limit,
                totalPages
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error searching recipes", "Unable to perform recipe search. Please try again later.");
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.ServiceInterfaces;
using System;
using System.Threading.Tasks;

namespace TheCoffeeHand.Controllers
{
    /// <summary>
    /// Controller for managing recipes.
    /// </summary>
    [Route("api/recipes")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        private readonly IRecipeService _recipeService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecipeController"/> class.
        /// </summary>
        /// <param name="recipeService">The recipe service.</param>
        public RecipeController(IRecipeService recipeService)
        {
            _recipeService = recipeService;
        }

        /// <summary>
        /// Creates a new recipe.
        /// </summary>
        /// <param name="recipeDTO">The recipe details.</param>
        /// <returns>The created recipe.</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Firebase,Jwt", Roles = "Admin")]
        public async Task<IActionResult> CreateRecipe([FromBody] RecipeRequestDTO recipeDTO)
        {
            var recipe = await _recipeService.CreateRecipeAsync(recipeDTO);
            return CreatedAtAction(nameof(GetRecipeById), new { id = recipe.Id }, recipe);
        }

        /// <summary>
        /// Retrieves a recipe by its ID.
        /// </summary>
        /// <param name="id">The recipe ID.</param>
        /// <returns>The recipe details.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRecipeById(Guid id)
        {
            var recipe = await _recipeService.GetRecipeByIdAsync(id);
            if (recipe == null)
            {
                return NotFound(new { message = "Recipe not found." });
            }
            return Ok(recipe);
        }

        /// <summary>
        /// Retrieves all recipes.
        /// </summary>
        /// <returns>A list of recipes.</returns>
        [HttpGet]
        public async Task<IActionResult> GetRecipes()
        {
            var recipes = await _recipeService.GetRecipesAsync();
            return Ok(recipes);
        }

        /// <summary>
        /// Retrieves paginated recipes.
        /// </summary>
        /// <param name="pageNumber">The page number (default is 1).</param>
        /// <param name="pageSize">The page size (default is 10).</param>
        /// <returns>A paginated list of recipes.</returns>
        [HttpGet("paginated")]
        public async Task<IActionResult> GetRecipesPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest(new { message = "pageNumber and pageSize must be greater than 0." });
            }

            var recipes = await _recipeService.GetRecipesAsync(pageNumber, pageSize);
            return Ok(recipes);
        }

        /// <summary>
        /// Updates a recipe by its ID.
        /// </summary>
        /// <param name="id">The recipe ID.</param>
        /// <param name="recipeDTO">The updated recipe details.</param>
        /// <returns>The updated recipe.</returns>
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = "Firebase,Jwt", Roles = "Admin")]
        public async Task<IActionResult> UpdateRecipe(Guid id, [FromBody] RecipeRequestDTO recipeDTO)
        {
            var existingRecipe = await _recipeService.GetRecipeByIdAsync(id);
            if (existingRecipe == null)
            {
                return NotFound(new { message = "Recipe not found." });
            }

            var updatedRecipe = await _recipeService.UpdateRecipeAsync(id, recipeDTO);
            return Ok(updatedRecipe);
        }

        /// <summary>
        /// Deletes a recipe by its ID.
        /// </summary>
        /// <param name="id">The recipe ID.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = "Firebase,Jwt", Roles = "Admin")]
        public async Task<IActionResult> DeleteRecipe(Guid id)
        {
            var existingRecipe = await _recipeService.GetRecipeByIdAsync(id);
            if (existingRecipe == null)
            {
                return NotFound(new { message = "Recipe not found." });
            }

            await _recipeService.DeleteRecipeAsync(id);
            return NoContent();
        }
    }
}

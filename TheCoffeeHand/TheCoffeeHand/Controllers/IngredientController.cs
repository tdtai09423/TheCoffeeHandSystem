using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.ServiceInterfaces;

namespace TheCoffeeHand.Controllers
{
    /// <summary>
    /// Controller for managing ingredients.
    /// </summary>
    [Route("api/ingredient")]
    [ApiController]
    public class IngredientController : ControllerBase
    {
        private readonly IIngredientService _ingredientService;

        /// <summary>
        /// Initializes a new instance of the <see cref="IngredientController"/> class.
        /// </summary>
        /// <param name="ingredientService">Service for handling ingredient operations.</param>
        public IngredientController(IIngredientService ingredientService)
        {
            _ingredientService = ingredientService;
        }

        /// <summary>
        /// Retrieves a paginated list of ingredients.
        /// </summary>
        /// <param name="pageNumber">The page number (default is 1).</param>
        /// <param name="pageSize">The number of items per page (default is 10).</param>
        /// <returns>Returns a paginated list of ingredients.</returns>
        [HttpGet("paginated")]
        public async Task<IActionResult> GetIngredients([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest("pageNumber and pageSize must be greater than 0.");
            }
            var ingredients = await _ingredientService.GetIngredientsAsync(pageNumber, pageSize);
            return Ok(ingredients);
        }

        /// <summary>
        /// Retrieves an ingredient by its ID.
        /// </summary>
        /// <param name="id">The ID of the ingredient.</param>
        /// <returns>Returns the ingredient if found, otherwise NotFound.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetIngredientById(Guid id)
        {
            var ingredient = await _ingredientService.GetIngredientByIdAsync(id);
            if (ingredient == null)
                return NotFound(new { message = "Ingredient not found." });
            return Ok(ingredient);
        }

        /// <summary>
        /// Creates a new ingredient.
        /// </summary>
        /// <param name="ingredientDTO">The ingredient data transfer object.</param>
        /// <returns>Returns the created ingredient.</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Firebase,Jwt", Roles = "Admin")]
        public async Task<IActionResult> CreateIngredient([FromBody] IngredientRequestDTO ingredientDTO)
        {
            var createdIngredient = await _ingredientService.CreateIngredientAsync(ingredientDTO);
            return CreatedAtAction(nameof(GetIngredientById), new { id = createdIngredient.Id }, createdIngredient);
        }

        /// <summary>
        /// Updates an existing ingredient.
        /// </summary>
        /// <param name="id">The ID of the ingredient to update.</param>
        /// <param name="ingredientDTO">The updated ingredient data.</param>
        /// <returns>Returns the updated ingredient if successful, otherwise NotFound.</returns>
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = "Firebase,Jwt", Roles = "Admin")]
        public async Task<IActionResult> UpdateIngredient(Guid id, [FromBody] IngredientResponseDTO ingredientDTO)
        {
            var updatedIngredient = await _ingredientService.UpdateIngredientAsync(id, ingredientDTO);
            if (updatedIngredient == null)
                return NotFound(new { message = "Ingredient not found." });
            return Ok(updatedIngredient);
        }

        /// <summary>
        /// Deletes an ingredient by its ID.
        /// </summary>
        /// <param name="id">The ID of the ingredient to delete.</param>
        /// <returns>Returns NoContent if deletion is successful.</returns>
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = "Firebase,Jwt", Roles = "Admin")]
        public async Task<IActionResult> DeleteIngredient(Guid id)
        {
            await _ingredientService.DeleteIngredientAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Retrieves an ingredient by its Name.
        /// </summary>
        /// <param name="name">The name of the ingredient.</param>
        /// <returns>Returns the ingredient if found, otherwise NotFound.</returns>
        [HttpGet("by-name")]
        public async Task<IActionResult> GetIngredientByName([FromQuery] string name) {
            var ingredient = await _ingredientService.GetIngredientByNameAsync(name);
            if (ingredient == null)
                return NotFound(new { message = "Ingredient not found." });
            return Ok(ingredient);
        }
    }
}

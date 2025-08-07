using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.ServiceInterfaces;

namespace TheCoffeeHand.Controllers {
    /// <summary>
    /// Controller for managing drinks.
    /// </summary>
    [Route("api/drink")]
    [ApiController]
    public class DrinkController: ControllerBase {
        private readonly IDrinkService _drinkService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrinkController"/> class.
        /// </summary>
        /// <param name="drinkService">Service for handling drink operations.</param>
        public DrinkController(IDrinkService drinkService) {
            _drinkService = drinkService;
        }

        /// <summary>
        /// Creates a new drink.
        /// </summary>
        /// <param name="drinkDTO">The drink data transfer object.</param>
        /// <returns>Returns the created drink.</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Firebase,Jwt", Roles = "Admin")]
        public async Task<IActionResult> CreateDrink([FromBody] DrinkRequestDTO drinkDTO) {
            var result = await _drinkService.CreateDrinkAsync(drinkDTO);
            return CreatedAtAction(nameof(GetDrinkById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Retrieves a drink by its ID.
        /// </summary>
        /// <param name="id">The ID of the drink.</param>
        /// <returns>Returns the drink if found; otherwise, returns NotFound.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDrinkById(Guid id) {
            var drink = await _drinkService.GetDrinkByIdAsync(id);
            if (drink == null)
                return NotFound(new { message = "Drink not found" });
            return Ok(drink);
        }

        /// <summary>
        /// Retrieves a paginated list of drinks.
        /// </summary>
        /// <param name="pageNumber">The page number (default is 1).</param>
        /// <param name="pageSize">The number of items per page (default is 10).</param>
        /// <returns>Returns a paginated list of drinks.</returns>
        [HttpGet("paginated")]
        public async Task<IActionResult> GetDrinksPaginated(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10) {
            if (pageNumber <= 0 || pageSize <= 0) {
                return BadRequest("pageNumber and pageSize must be greater than 0.");
            }
            var drinks = await _drinkService.GetDrinksAsync(pageNumber, pageSize);
            return Ok(drinks);
        }

        /// <summary>
        /// Retrieves a paginated list of avaiable drinks.
        /// </summary>
        /// <param name="drinkName"></param>
        /// <param name="categoryName"></param>
        /// <param name="pageNumber">The page number (default is 1).</param>
        /// <param name="pageSize">The number of items per page (default is 10).</param>
        /// <returns>Returns a paginated list of drinks.</returns>
        [HttpGet("paginated/available")]
        public async Task<IActionResult> GetAvaiableDrinksPaginated(
            [FromQuery] string? drinkName,
            [FromQuery] string? categoryName,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10) {
            if (pageNumber <= 0 || pageSize <= 0) {
                return BadRequest("pageNumber and pageSize must be greater than 0.");
            }
            var drinks = await _drinkService.GetDrinksAvailableAsync(pageNumber, pageSize, drinkName, categoryName);
            return Ok(drinks);
        }

        /// <summary>
        /// Updates an existing drink.
        /// </summary>
        /// <param name="id">The ID of the drink to update.</param>
        /// <param name="drinkDTO">The updated drink data.</param>
        /// <returns>Returns the updated drink.</returns>
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = "Firebase,Jwt", Roles = "Admin")]
        public async Task<IActionResult> UpdateDrink(Guid id, [FromBody] DrinkRequestDTO drinkDTO) {
            var updatedDrink = await _drinkService.UpdateDrinkAsync(id, drinkDTO);
            return Ok(updatedDrink);
        }

        /// <summary>
        /// Deletes a drink by its ID.
        /// </summary>
        /// <param name="id">The ID of the drink to delete.</param>
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = "Firebase,Jwt", Roles = "Admin")]
        public async Task<IActionResult> DeleteDrink(Guid id) {
            await _drinkService.DeleteDrinkAsync(id);
            return Ok(new { message = "Drink deleted successfully." });
        }

        /// <summary>
        /// Retrieves all drinks by category name.
        /// </summary>
        /// <param name="categoryName">The name of the category.</param>
        /// <returns>Returns the list of drinks in the specified category.</returns>
        [HttpGet("by-category/{categoryName}")]
        public async Task<IActionResult> GetDrinksByCategory(string categoryName) {
            if (string.IsNullOrWhiteSpace(categoryName)) {
                return BadRequest(new { message = "Category name is required." });
            }

            var drinks = await _drinkService.GetDrinksByCategoryAsync(categoryName);

            return Ok(drinks);
        }

    }
}

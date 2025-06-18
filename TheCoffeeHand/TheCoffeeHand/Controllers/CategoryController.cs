using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.ServiceInterfaces;

namespace TheCoffeeHand.Controllers
{
    /// <summary>
    /// Controller for managing categories.
    /// </summary>
    [Route("api/categories")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryController"/> class.
        /// </summary>
        /// <param name="categoryService">Service for handling category operations.</param>
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Creates a new category.
        /// </summary>
        /// <param name="categoryDTO">The category data transfer object.</param>
        /// <returns>Returns the created category.</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Firebase,Jwt", Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryRequestDTO categoryDTO)
        {
            var category = await _categoryService.CreateCategoryAsync(categoryDTO);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
        }

        /// <summary>
        /// Retrieves a category by its ID.
        /// </summary>
        /// <param name="id">The ID of the category.</param>
        /// <returns>Returns the category if found.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(Guid id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            return Ok(category);
        }

        /// <summary>
        /// Retrieves a paginated list of categories.
        /// </summary>
        /// <param name="pageNumber">The page number (default is 1).</param>
        /// <param name="pageSize">The number of items per page (default is 10).</param>
        /// <returns>Returns a paginated list of categories.</returns>
        [HttpGet("paginated")]
        public async Task<IActionResult> GetAllCategories(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest("pageNumber and pageSize must be greater than 0.");
            }
            var paginatedCategories = await _categoryService.GetAllCategoriesAsync(pageNumber, pageSize);
            return Ok(paginatedCategories);
        }

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        /// <param name="id">The ID of the category to update.</param>
        /// <param name="categoryDTO">The updated category data.</param>
        /// <returns>Returns the updated category.</returns>
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = "Firebase,Jwt", Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CategoryRequestDTO categoryDTO)
        {
            var updatedCategory = await _categoryService.UpdateCategoryAsync(id, categoryDTO);
            return Ok(updatedCategory);
        }

        /// <summary>
        /// Deletes a category by its ID.
        /// </summary>
        /// <param name="id">The ID of the category to delete.</param>
        /// <returns>Returns a success message upon deletion.</returns>
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = "Firebase,Jwt", Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            await _categoryService.DeleteCategoryAsync(id);
            return Ok(new { message = "Category deleted successfully." });
        }
    }
}

using Interfracture.PaggingItems;
using Services.DTOs;

namespace Services.ServiceInterfaces
{
    public interface IRecipeService 
    {
        Task<RecipeResponseDTO> CreateRecipeAsync(RecipeRequestDTO recipeDTO);
        Task<RecipeResponseDTO> GetRecipeByIdAsync(Guid id);
        Task<List<RecipeResponseDTO>> GetRecipesAsync();
        Task<PaginatedList<RecipeResponseDTO>> GetRecipesAsync(int pageNumber, int pageSize);
        Task<RecipeResponseDTO> UpdateRecipeAsync(Guid id, RecipeRequestDTO recipeDTO);
        Task DeleteRecipeAsync(Guid id);
    }
}

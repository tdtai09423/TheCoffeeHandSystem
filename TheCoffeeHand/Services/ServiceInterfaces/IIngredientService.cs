using Interfracture.PaggingItems;
using Services.DTOs;

namespace Services.ServiceInterfaces
{
    public interface IIngredientService
    {
        Task<IngredientResponseDTO> CreateIngredientAsync(IngredientRequestDTO ingredientDTO);
        Task<IngredientResponseDTO> GetIngredientByIdAsync(Guid id);
        Task<PaginatedList<IngredientResponseDTO>> GetIngredientsAsync(int pageNumber, int pageSize);
        Task<IngredientResponseDTO> UpdateIngredientAsync(Guid id, IngredientResponseDTO ingredientDTO);
        Task DeleteIngredientAsync(Guid id);
        Task<IngredientResponseDTO> GetIngredientByNameAsync(string name);
    }
}

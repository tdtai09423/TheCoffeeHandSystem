using Interfracture.PaggingItems;
using Services.DTOs;

namespace Services.ServiceInterfaces
{
    public interface IDrinkService
    {
        Task<DrinkResponseDTO> CreateDrinkAsync(DrinkRequestDTO drinkDTO);
        Task<DrinkResponseDTO> GetDrinkByIdAsync(Guid id);
        Task<List<DrinkResponseDTO>> GetDrinksAsync();
        Task<PaginatedList<DrinkResponseDTO>> GetDrinksAvailableAsync(int pageNumber, int pageSize, string? drinkName = null, string? categoryName = null);
        Task<PaginatedList<DrinkResponseDTO>> GetDrinksAsync(int pageNumber, int pageSize);
        Task<DrinkResponseDTO> UpdateDrinkAsync(Guid id, DrinkRequestDTO drinkDTO);
        Task DeleteDrinkAsync(Guid id);

    }
}

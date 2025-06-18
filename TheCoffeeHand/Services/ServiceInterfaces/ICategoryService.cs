using Domain.Base;
using Interfracture.PaggingItems;
using Services.DTOs;


namespace Services.ServiceInterfaces
{
    public interface ICategoryService
    {
        Task<CategoryResponseDTO> CreateCategoryAsync(CategoryRequestDTO categoryDTO);
        Task<CategoryResponseDTO> GetCategoryByIdAsync(Guid id);
        Task<PaginatedList<CategoryResponseDTO>> GetAllCategoriesAsync(int pageNumber, int pageSize);
        Task<CategoryResponseDTO> UpdateCategoryAsync(Guid id, CategoryRequestDTO categoryDTO);
        Task<bool> DeleteCategoryAsync(Guid id);
    }
}

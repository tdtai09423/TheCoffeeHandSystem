
namespace Services.ServiceInterfaces
{
    public interface ICrudServiceBase<TRequestDTO, TResponseDTO>
    {
        Task<TResponseDTO> CreateAsync(TRequestDTO dto);
        Task<TResponseDTO> GetByIdAsync(Guid id);
        Task<TResponseDTO> UpdateAsync(Guid id, TRequestDTO dto);
        Task DeleteAsync(Guid id);
    }
}

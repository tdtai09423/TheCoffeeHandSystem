using Domain.Base;
using Interfracture.PaggingItems;
using Services.DTOs;

namespace Services.ServiceInterfaces
{
    public interface IUserServices
    {
        Task<PaginatedList<UserDTO>> SearchUsersAsync(
            string? firstName,
            string? lastName,
            string? phone,
            string? email,
            string? roleName,
            int pageNumber,
            int pageSize);
        Task<UserDTO?> GetUserByIdAsync(string userId);
        Task<UserDTO?> GetCurrentUserAsync();
        public string? GetCurrentUserId();
        Task<UserDTO> UpdateUser(Guid id, UserRequestDTO userDto);
        Task DeleteUserAsync(Guid id);
    }
}

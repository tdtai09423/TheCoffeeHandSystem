using Interfracture.PaggingItems;
using Services.DTOs;

namespace Services.ServiceInterfaces
{
    public interface IOrderDetailService
    {
        Task<OrderDetailResponselDTO> CreateOrderDetailAsync(OrderDetailRequestDTO dto);
        Task<OrderDetailResponselDTO> GetOrderDetailByIdAsync(Guid id);
        Task<List<OrderDetailResponselDTO>> GetOrderDetailsAsync();
        Task<PaginatedList<OrderDetailResponselDTO>> GetOrderDetailsAsync(int pageNumber, int pageSize);
        Task<OrderDetailResponselDTO> UpdateOrderDetailAsync(Guid id, OrderDetailRequestDTO dto);
        Task DeleteOrderDetailAsync(Guid id);
        Task<bool> RemoveFromCartAsync(Guid orderDetailId);
    }
}

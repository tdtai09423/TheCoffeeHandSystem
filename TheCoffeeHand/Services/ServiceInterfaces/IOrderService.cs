using Interfracture.PaggingItems;
using Services.DTOs;

namespace Services.ServiceInterfaces
{
    public interface IOrderService
    {
        Task<OrderResponseDTO> CreateOrderAsync(OrderRequestDTO orderDTO);
        Task<OrderResponseDTO> GetOrderByIdAsync(Guid id);
        Task<PaginatedList<OrderResponseDTO>> GetOrdersAsync(int pageNumber, int pageSize, Guid? userId, DateTimeOffset? dateStart, DateTimeOffset? dateEnd);
        Task<OrderResponseDTO> UpdateOrderAsync(Guid id, OrderRequestDTO orderDTO);
        Task DeleteOrderAsync(Guid id);
        Task<OrderResponseDTO> GetCartAsync();
        Task ConfirmOrderAsync(Guid orderId);
        Task CancelOrderAsync(Guid orderId);
        Task TestSendMessage();
        Task CompleteOrderAsync(Guid orderId);
    }
}

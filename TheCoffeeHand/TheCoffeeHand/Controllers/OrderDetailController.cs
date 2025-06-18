using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.ServiceInterfaces;

namespace TheCoffeeHand.Controllers
{
    /// <summary>
    /// Controller for managing order details.
    /// </summary>
    [Route("api/order-details")]
    [ApiController]
    public class OrderDetailController : ControllerBase
    {
        private readonly IOrderDetailService _orderDetailService;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderDetailController"/> class.
        /// </summary>
        /// <param name="orderDetailService">The order detail service.</param>
        public OrderDetailController(IOrderDetailService orderDetailService)
        {
            _orderDetailService = orderDetailService;
        }

        /// <summary>
        /// Creates a new order detail to cart.
        /// </summary>
        /// <param name="dto">The order detail data transfer object.</param>
        /// <returns>The created order detail.</returns>
        [HttpPost("cart")]
        public async Task<IActionResult> CreateOrderDetail([FromBody] OrderDetailRequestDTO dto)
        {
            var createdOrderDetail = await _orderDetailService.CreateOrderDetailAsync(dto);
            return CreatedAtAction(nameof(GetOrderDetailById), new { id = createdOrderDetail.Id }, createdOrderDetail);
        }

        /// <summary>
        /// Gets an order detail by ID.
        /// </summary>
        /// <param name="id">The order detail ID.</param>
        /// <returns>The order detail if found; otherwise, a not found response.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetailById(Guid id)
        {
            var orderDetail = await _orderDetailService.GetOrderDetailByIdAsync(id);
            if (orderDetail == null)
                return NotFound("Order detail not found.");

            return Ok(orderDetail);
        }

        /// <summary>
        /// Retrieves all order details.
        /// </summary>
        /// <returns>A list of order details.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllOrderDetails()
        {
            var orderDetails = await _orderDetailService.GetOrderDetailsAsync();
            return Ok(orderDetails);
        }

        /// <summary>
        /// Retrieves paginated order details.
        /// </summary>
        /// <param name="pageNumber">The page number (default is 1).</param>
        /// <param name="pageSize">The page size (default is 10).</param>
        /// <returns>A paginated list of order details.</returns>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaginatedOrderDetails([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var paginatedOrderDetails = await _orderDetailService.GetOrderDetailsAsync(pageNumber, pageSize);
            return Ok(paginatedOrderDetails);
        }

        /// <summary>
        /// Updates an existing order detail.
        /// </summary>
        /// <param name="id">The order detail ID.</param>
        /// <param name="dto">The updated order detail data.</param>
        /// <returns>The updated order detail.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrderDetail(Guid id, [FromBody] OrderDetailRequestDTO dto)
        {
            var updatedOrderDetail = await _orderDetailService.UpdateOrderDetailAsync(id, dto);
            return Ok(updatedOrderDetail);
        }

        /// <summary>
        /// Deletes an order detail by ID.
        /// </summary>
        /// <param name="id">The order detail ID.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderDetail(Guid id)
        {
            await _orderDetailService.DeleteOrderDetailAsync(id);
            return NoContent();
        }

        /// <summary>
        /// remove an order detail from a cart.
        /// </summary>
        [HttpPut("cart/{id}")]
        public async Task<IActionResult> RemoveOrderDetailFromCart(Guid id)
        {
            await _orderDetailService.RemoveFromCartAsync(id);
            return Ok();
        }
    }
}

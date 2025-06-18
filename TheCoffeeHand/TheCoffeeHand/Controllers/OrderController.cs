using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.ServiceInterfaces;

namespace TheCoffeeHand.Controllers
{
    /// <summary>
    /// Controller for managing orders.
    /// </summary>
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderController"/>.
        /// </summary>
        /// <param name="orderService">The order service.</param>
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Creates a new order.
        /// </summary>
        /// <param name="orderDTO">The order data.</param>
        /// <returns>The created order.</returns>
        /// <response code="201">Returns the newly created order.</response>
        /// <response code="400">If the request data is invalid.</response>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequestDTO orderDTO)
        {
            if (orderDTO == null)
                return BadRequest("Invalid order data.");

            var createdOrder = await _orderService.CreateOrderAsync(orderDTO);
            return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.Id }, createdOrder);
        }

        /// <summary>
        /// Retrieves an order by ID.
        /// </summary>
        /// <param name="id">The order ID.</param>
        /// <returns>The requested order.</returns>
        /// <response code="200">Returns the order if found.</response>
        /// <response code="404">If the order is not found.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound("Order not found.");

            return Ok(order);
        }

        /// <summary>
        /// Retrieves cart for current user.
        /// </summary>
        [HttpGet("cart")]
        [Authorize(AuthenticationSchemes = "Firebase,Jwt")]
        public async Task<IActionResult> GetCart()
        {
            var order = await _orderService.GetCartAsync();

            return Ok(order);
        }

        /// <summary>
        /// Retrieves paginated orders, optionally filtered by user and dateStart.
        /// </summary>
        /// <param name="userId">Optional user ID filter.</param>
        /// <param name="dateStart">Optional dateStart filter.</param>
        /// <param name="dateEnd">Optional dateEnd filter.</param>
        /// <param name="pageNumber">Page number (default: 1).</param>
        /// <param name="pageSize">Page size (default: 10).</param>
        /// <returns>Paginated list of orders.</returns>
        /// <response code="200">Returns the list of orders.</response>
        /// <response code="400">If pageNumber or pageSize is invalid.</response>
        [HttpGet("paginated")]
        public async Task<IActionResult> GetOrders(Guid? userId, [FromQuery] DateTimeOffset? dateStart, [FromQuery] DateTimeOffset? dateEnd, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return BadRequest("Page number and page size must be greater than zero.");

            var paginatedOrders = await _orderService.GetOrdersAsync(pageNumber, pageSize, userId, dateStart, dateEnd);
            return Ok(paginatedOrders);
        }

        /// <summary>
        /// Updates an existing order.
        /// </summary>
        /// <param name="id">The order ID.</param>
        /// <param name="orderDTO">The updated order data.</param>
        /// <returns>The updated order.</returns>
        /// <response code="200">If the update is successful.</response>
        /// <response code="404">If the order is not found.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] OrderRequestDTO orderDTO)
        {
            var updatedOrder = await _orderService.UpdateOrderAsync(id, orderDTO);
            if (updatedOrder == null)
                return NotFound("Order not found.");

            return Ok(updatedOrder);
        }

        /// <summary>
        /// Deletes an order by ID.
        /// </summary>
        /// <param name="id">The order ID.</param>
        /// <returns>No content.</returns>
        /// <response code="204">If the order is successfully deleted.</response>
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = "Firebase,Jwt", Roles = "Admin")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            await _orderService.DeleteOrderAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Comfirm an order by ID.
        /// </summary>
        /// <param name="id">The order ID.</param>
        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> ConfirmOrder(Guid id)
        {
            await _orderService.ConfirmOrderAsync(id);
            return Ok("Order confirmed successfully.");
        }

        /// <summary>
        /// Cancel an order by ID.
        /// </summary>
        /// <param name="id">The order ID.</param>
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            await _orderService.CancelOrderAsync(id);
            return Ok("Order canceled successfully.");
        }

        /// <summary>
        /// Sends a test message for the order.
        /// </summary>
        /// <returns>The current cart order.</returns>
        [HttpPost("test-message")]
        public async Task<IActionResult> SendOrderMessage() {
            await _orderService.TestSendMessage();

            return Ok("grasped");
        }

        /// <summary>
        /// Completes an order by setting status to Done.
        /// </summary>
        /// <param name="id">The order ID.</param>
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteOrder(Guid id) {
            await _orderService.CompleteOrderAsync(id);
            return Ok("Order completed successfully.");
        }

    }
}

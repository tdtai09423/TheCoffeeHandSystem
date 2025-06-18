using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.ServiceInterfaces;
using System.Threading.Tasks;

namespace TheCoffeeHand.Controllers
{
    /// <summary>
    /// Controller for managing user-related operations.
    /// </summary>
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserServices _userServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="userServices">The user services.</param>
        public UserController(IUserServices userServices)
        {
            _userServices = userServices;
        }

        /// <summary>
        /// Searches users based on filters like name, phone, email, and role.
        /// </summary>
        /// <param name="firstName">The first name filter.</param>
        /// <param name="lastName">The last name filter.</param>
        /// <param name="phone">The phone number filter.</param>
        /// <param name="email">The email filter.</param>
        /// <param name="roleName">The role name filter.</param>
        /// <param name="pageNumber">The page number (default is 1).</param>
        /// <param name="pageSize">The page size (default is 10).</param>
        /// <returns>A paginated list of users.</returns>
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers(
            [FromQuery] string? firstName,
            [FromQuery] string? lastName, // Renamed for consistency
            [FromQuery] string? phone,
            [FromQuery] string? email,
            [FromQuery] string? roleName,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest(new { message = "pageNumber and pageSize must be greater than 0." });
            }

            var users = await _userServices.SearchUsersAsync(firstName, lastName, phone, email, roleName, pageNumber, pageSize);
            return Ok(users);
        }

        /// <summary>
        /// Retrieves a user by their ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The user details if found; otherwise, a not found response.</returns>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var user = await _userServices.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }
            return Ok(user);
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(Guid userId, UserRequestDTO updateUser)
        {
            var user = await _userServices.UpdateUser(userId, updateUser);
            return Ok(user);
        }
    }
}

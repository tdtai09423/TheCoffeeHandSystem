using Microsoft.AspNetCore.Mvc;
using Services.DTOs;
using Services.ServiceInterfaces;

namespace TheCoffeeHand.Controllers
{
    /// <summary>
    /// Controller for managing notifications using Firebase Cloud Messaging (FCM).
    /// </summary>
    [Route("api/notifications")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IFCMService _fcmService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationController"/> class.
        /// </summary>
        /// <param name="fcmService">The FCM service for sending notifications.</param>
        public NotificationController(IFCMService fcmService)
        {
            _fcmService = fcmService;
        }

        /// <summary>
        /// Sends a notification to a specified device.
        /// </summary>
        /// <param name="request">The notification request DTO containing device token, title, and body.</param>
        /// <returns>Returns a success message if the notification is sent, otherwise a bad request response.</returns>
        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationRequestDTO request)
        {
            bool success = await _fcmService.SendNotificationAsync(request.DeviceToken, request.Title, request.Body);

            if (success)
                return Ok(new { message = "Notification sent successfully!" });

            return BadRequest(new { message = "Failed to send notification." });
        }
    }
}

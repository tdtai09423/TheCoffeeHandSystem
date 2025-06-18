using Microsoft.AspNetCore.Mvc;
using Services.ServiceInterfaces;
using Services.Services;

namespace TheCoffeeHand.Controllers
{
    /// <summary>
    /// Controller for handling image-related operations.
    /// </summary>
    [ApiController]
    [Route("api/image")]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageController"/> class.
        /// </summary>
        /// <param name="imageService">The image service.</param>
        public ImageController(IImageService imageService)
        {
            _imageService = imageService;
        }

        /// <summary>
        /// Uploads an image file.
        /// </summary>
        /// <param name="file">The image file to upload.</param>
        /// <returns>The URL of the uploaded image.</returns>
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using var stream = file.OpenReadStream();
            var imageUrl = await _imageService.UploadImageAsync(stream, file.FileName);

            return Ok(new { Url = imageUrl });
        }
    }
}

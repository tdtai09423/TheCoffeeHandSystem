using System.ComponentModel.DataAnnotations;

namespace Services.DTOs
{
    public class DrinkResponseDTO
    {
        public Guid? Id { get; set; }
        public string? Description { get; set; }
        public double Price { get; set; }
        public string? ImageUrl { get; set; }
        public Guid? CategoryId { get; set; }
        public CategoryResponseDTO? Category { get; set; }
        public List<RecipeResponseDTO>? Recipe { get; set; }
        public string? Name { get; set; }
        public Boolean? isAvailable { get; set; }
    }

    public class DrinkRequestDTO
    {
        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters.")]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0.")]
        public required double Price { get; set; }

        [Url(ErrorMessage = "Invalid URL format.")]
        public string? ImageUrl { get; set; }

        public Guid? CategoryId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public required string Name { get; set; }

        public bool? IsAvailable { get; set; } = true;
    }

}

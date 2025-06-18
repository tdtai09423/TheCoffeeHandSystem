using System.ComponentModel.DataAnnotations;

namespace Services.DTOs
{
    public class IngredientResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; } = 0;
        public double Price { get; set; } = 0;
    }

    public class IngredientRequestDTO
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public required string Name { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be greater than or equal to 0.")]
        public int Quantity { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0.")]
        public double Price { get; set; } = 0;
    }
}

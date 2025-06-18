using Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Drink : BaseEntity
    {
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public double Price { get; set; }
        public string? ImageUrl { get; set; }
        public Guid? CategoryId { get; set; }
        public virtual Category? Category { get; set; }
        [Required(ErrorMessage = "Drink name is required.")]
        [MinLength(4, ErrorMessage = "Drink name must be greater than 3 characters.")]
        [MaxLength(100, ErrorMessage = "Drink name cannot exceed 100 characters.")]
        [RegularExpression(@"^([A-Z0-9][a-zA-Z0-9]*\s?)*$",
            ErrorMessage = "Each word must start with a capital letter or number and must not contain special characters.")]
        public string? Name { get; set; }
        public Boolean? isAvailable { get; set; }
        // Navigation properties
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
    }
}

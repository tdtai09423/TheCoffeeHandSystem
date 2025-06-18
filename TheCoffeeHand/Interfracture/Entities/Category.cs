using Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Category : BaseEntity
    {
        [Required(ErrorMessage = "Category name is required.")]
        [MinLength(4, ErrorMessage = "Category name must be greater than 3 characters.")]
        [MaxLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
        [RegularExpression(@"^([A-Z0-9][a-zA-Z0-9]*\s?)*$",
            ErrorMessage = "Each word must start with a capital letter or number and must not contain special characters.")]
        public string? Name { get; set; }
        public virtual ICollection<Drink> Drinks { get; set; } = new List<Drink>();
    }
}

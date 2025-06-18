using Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Recipe : BaseEntity
    {
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be greater than or equal to 0.")]
        public required int Quantity { get; set; }
        public required Guid IngredientId { get; set; }
        public required Guid DrinkId { get; set; }
        public virtual Drink? Drink { get; set; }
        public virtual Ingredient? Ingredient { get; set; }
    }
}

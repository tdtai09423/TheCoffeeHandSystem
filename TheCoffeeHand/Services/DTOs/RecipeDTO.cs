namespace Services.DTOs
{
    public class RecipeResponseDTO
    {
        public Guid Id { get; set; }
        public required int Quantity { get; set; }
        public required Guid IngredientId { get; set; }
        public required Guid DrinkId { get; set; }
    }
    public class RecipeRequestDTO
    {
        public required int Quantity { get; set; }
        public required Guid IngredientId { get; set; }
        public required Guid DrinkId { get; set; }
    }
}

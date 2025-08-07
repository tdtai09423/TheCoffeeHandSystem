using Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Services.DTOs
{
    public class OrderDetailResponselDTO
    {
        public Guid Id { get; set; }
        public int Total { get; set; } = 0;
        public string Note { get; set; } = string.Empty;
        public Guid? OrderId { get; set; }
        public Guid? DrinkId { get; set; }
        public DrinkDTO Drink { get; set; }
    }
    public class OrderDetailRequestDTO
    {
        [Range(0, int.MaxValue, ErrorMessage = "Total must be greater than or equal to 0.")]
        public int Total { get; set; } = 0;
        public string Note { get; set; } = string.Empty;
        public required Guid OrderId { get; set; }
        public required Guid DrinkId { get; set; }
    }
}

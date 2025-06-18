using Domain.Base;

namespace Domain.Entities
{
    public class OrderDetail : BaseEntity
    {
        public int Total { get; set; } = 0; 
        public string Note { get; set; } = string.Empty;
        public Guid? OrderId { get; set; } 
        public virtual Order? Order { get; set; }
        public Guid? DrinkId { get; set; }
        public virtual Drink? Drink { get; set; }
    }
}

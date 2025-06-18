using Core.Constants.Enum;
using Domain.Base;
using Interfracture.Entities;

namespace Domain.Entities
{
    public class Order : BaseEntity
    {
        public DateTimeOffset? Date { get; set; }
        public EnumOrderStatus? Status { get; set; }
        public double TotalPrice { get; set; } = 0;
        public Guid? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        // Navigation properties
        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}

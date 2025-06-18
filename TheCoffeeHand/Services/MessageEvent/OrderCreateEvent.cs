using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.MessageEvent {
    public record OrderCreateEvent {
        public Guid OrderId { get; init; }
        public ICollection<OrderDetail>? OrderDetails { get; init; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services.MessageQueue {
    public class OrderMessage {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public List<DrinkMessage> Drinks { get; set; }
    }
}

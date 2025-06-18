using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services.MessageQueue {
    public class DrinkMessage {
        public Guid DrinkId { get; set; }
        public string DrinkName { get; set; }
        public int Quantity { get; set; }
    }
}

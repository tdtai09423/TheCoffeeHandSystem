using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.MessageBroker {
    public sealed class MessageBrokerSetting {
        public string? Host { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
}

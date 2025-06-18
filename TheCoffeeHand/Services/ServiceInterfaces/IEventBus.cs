using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfracture.MessageBroker {
    public interface IEventBus {
        Task PublishAsync<T>(T message);
    }
}

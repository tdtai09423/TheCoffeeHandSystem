using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.ServiceInterfaces {
    public interface IRabbitMQConsumerService {
        Task StartConsumingAsync(string queueName);
    }
}




namespace Services.ServiceInterfaces {
    public interface IRabbitMQService {
        Task SendMessageAsync(string queueName, string message);
    }

}

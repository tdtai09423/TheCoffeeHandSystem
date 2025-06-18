using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using Services.ServiceInterfaces;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services {
    public class RabbitMQService: IRabbitMQService {
        private readonly ILogger<RabbitMQService> _logger;
        private readonly ConnectionFactory _factory;

        public RabbitMQService(ConnectionFactory factory, ILogger<RabbitMQService> logger) {
            _factory = factory;
            _logger = logger;
        }

        public async Task SendMessageAsync(string queueName, string message) {
            try {
                using var connection = await _factory.CreateConnectionAsync();
                using var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(queue: queueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(message);
                var properties = new BasicProperties();

                await channel.BasicPublishAsync(exchange: "",
                                     routingKey: queueName,
                                     mandatory: false,
                                     basicProperties: properties,
                                     body: body);

                //_logger.LogInformation($" [x] Sent message to {queueName}: {message}");
            } catch (Exception ex) {
                _logger.LogError($"Error sending message to {queueName}: {ex.Message}");
            }
        }
    }
}


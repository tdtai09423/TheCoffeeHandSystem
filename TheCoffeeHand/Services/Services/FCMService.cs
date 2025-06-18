using Microsoft.Extensions.Configuration;
using Services.ServiceInterfaces;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services.Services
{
    public class FCMService : IFCMService
    {
        private readonly string? _serverKey;
        private readonly string? _senderId;
        private readonly HttpClient _httpClient;

        public FCMService(IConfiguration configuration, HttpClient httpClient)
        {
            _serverKey = configuration["FCM:ServerKey"];
            _senderId = configuration["FCM:SenderId"];
            _httpClient = httpClient;
        }

        public async Task<bool> SendNotificationAsync(string deviceToken, string title, string body)
        {
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"key={_serverKey}");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Sender", $"id={_senderId}");

            var payload = new
            {
                to = deviceToken,
                notification = new
                {
                    title = title,
                    body = body
                }
            };

            string jsonPayload = JsonSerializer.Serialize(payload);
            var requestContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://fcm.googleapis.com/fcm/send", requestContent);
            return response.IsSuccessStatusCode;
        }
    }
}

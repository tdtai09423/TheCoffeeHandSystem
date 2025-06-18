namespace Services.DTOs
{
    public class NotificationRequestDTO
    {
        public required string DeviceToken { get; set; }
        public required string Title { get; set; }
        public required string Body { get; set; }
    }
}

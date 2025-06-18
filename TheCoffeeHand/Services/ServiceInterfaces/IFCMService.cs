namespace Services.ServiceInterfaces
{
    public interface IFCMService
    {
        Task<bool> SendNotificationAsync(string deviceToken, string title, string body);
    }
}

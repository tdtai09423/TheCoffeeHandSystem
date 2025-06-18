namespace Services.ServiceInterfaces
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(Stream fileStream, string fileName);
    }
}

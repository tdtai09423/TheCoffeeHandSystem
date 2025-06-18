namespace Services.ServiceInterfaces
{
    public interface IRedisCacheServices
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan expiration);
        Task RemoveAsync(string key);
        Task<IEnumerable<string>> GetKeysAsync(string pattern); // Add this method
        Task RemoveByPrefixAsync(string prefix);
        Task ClearAllCacheAsync();
    }

}

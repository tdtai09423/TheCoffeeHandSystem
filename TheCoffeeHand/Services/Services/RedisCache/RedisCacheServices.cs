using Services.ServiceInterfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace Services.Services.RedisCache
{
    public class RedisCacheServices : IRedisCacheServices
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public RedisCacheServices(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _db = _redis.GetDatabase();
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _db.StringGetAsync(key);
            if (value.IsNullOrEmpty) return default;
            return JsonSerializer.Deserialize<T>(value);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            var jsonData = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, jsonData, expiration);
        }

        public async Task RemoveAsync(string key)
        {
            await _db.KeyDeleteAsync(key);
        }

        // Get all keys that match a pattern
        public async Task<IEnumerable<string>> GetKeysAsync(string pattern)
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            return server.Keys(pattern: pattern).Select(k => k.ToString());
        }
        public async Task RemoveByPrefixAsync(string prefix)
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: $"{prefix}*").ToArray(); // Get all keys that start with prefix

            if (keys.Length > 0)
            {
                await _db.KeyDeleteAsync(keys); // Delete all matching keys
            }
        }

        // New method to clear all cache
        public async Task ClearAllCacheAsync()
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys().ToArray(); // Get all keys

            if (keys.Length > 0)
            {
                await _db.KeyDeleteAsync(keys); // Delete all keys
            }
        }
    }
}

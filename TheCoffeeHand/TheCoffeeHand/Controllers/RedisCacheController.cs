using Microsoft.AspNetCore.Mvc;
using Services.ServiceInterfaces;

namespace TheCoffeeHand.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RedisCacheController : ControllerBase
    {
        private readonly IRedisCacheServices _redisCacheServices;

        public RedisCacheController(IRedisCacheServices redisCacheServices)
        {
            _redisCacheServices = redisCacheServices;
        }

        // Clear all cache
        [HttpDelete("clear-all")]
        public async Task<IActionResult> ClearAllCache()
        {
            await _redisCacheServices.ClearAllCacheAsync();
            return Ok("All cache cleared successfully.");
        }

        // Remove cache by a specific key
        [HttpDelete("remove/{key}")]
        public async Task<IActionResult> RemoveCache(string key)
        {
            await _redisCacheServices.RemoveAsync(key);
            return Ok($"Cache with key '{key}' removed successfully.");
        }

        // Remove cache by prefix
        [HttpDelete("remove-by-prefix/{prefix}")]
        public async Task<IActionResult> RemoveCacheByPrefix(string prefix)
        {
            await _redisCacheServices.RemoveByPrefixAsync(prefix);
            return Ok($"All cache with prefix '{prefix}' removed successfully.");
        }
    }
}

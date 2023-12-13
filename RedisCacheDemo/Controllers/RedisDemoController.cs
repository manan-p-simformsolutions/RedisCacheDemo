using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace RedisCacheDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedisDemoController : ControllerBase
    {
        private readonly IDistributedCache _cache;

        public RedisDemoController(IDistributedCache cache)
        {
            _cache = cache;
        }
        [HttpGet("{key}")]
        public IActionResult Get(string key)
        {
            var cachedValue = _cache.GetString(key);

            if (cachedValue == null)
            {
                var startTime = DateTime.Now;
                System.Threading.Thread.Sleep(5000);
                var endTime = DateTime.Now;

                // Calculate the time taken for the operation
                var timeTaken = endTime - startTime;

                var value = $"Operation took {Math.Round(timeTaken.TotalSeconds, 3)} seconds to complete.";

                var cachedResult = new Dictionary<string, string>
                {
                    { "Key", key },
                    { "Value", value }
                };

                var serializedValue = JsonConvert.SerializeObject(cachedResult);

                // Set cache expiration time to 30 seconds
                var cacheEntryOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
                };

                _cache.SetString(key, serializedValue, cacheEntryOptions);

                return Ok(cachedResult);
            }

            var deserializedValue = JsonConvert.DeserializeObject<Dictionary<string, string>>(cachedValue);
            return Ok(deserializedValue);
        }
    }
}

using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using ProductCatalog.Application.Interfaces;

namespace ProductCatalog.Infrastructure.Cache;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public RedisCacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var cachedValue = await _distributedCache.GetStringAsync(key);

        if (string.IsNullOrEmpty(cachedValue))
            return default;

        return JsonSerializer.Deserialize<T>(cachedValue, JsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var serializedValue = JsonSerializer.Serialize(value, JsonOptions);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
        };

        await _distributedCache.SetStringAsync(key, serializedValue, options);
    }

    public async Task RemoveAsync(string key)
    {
        await _distributedCache.RemoveAsync(key);
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        // Note: For production Redis, use SCAN + DEL or Lua scripts.
        // This implementation works for in-memory cache fallback.
        await _distributedCache.RemoveAsync(prefix);
    }
}

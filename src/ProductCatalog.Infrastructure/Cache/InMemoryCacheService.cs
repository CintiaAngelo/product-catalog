using System.Collections.Concurrent;
using System.Text.Json;
using ProductCatalog.Application.Interfaces;

namespace ProductCatalog.Infrastructure.Cache;

public class InMemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, (string Value, DateTime? ExpiresAt)> _cache = new();
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public Task<T?> GetAsync<T>(string key)
    {
        if (!_cache.TryGetValue(key, out var entry))
            return Task.FromResult<T?>(default);

        if (entry.ExpiresAt.HasValue && entry.ExpiresAt < DateTime.UtcNow)
        {
            _cache.TryRemove(key, out _);
            return Task.FromResult<T?>(default);
        }

        var value = JsonSerializer.Deserialize<T>(entry.Value, JsonOptions);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var serialized = JsonSerializer.Serialize(value, JsonOptions);
        var expiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : (DateTime?)null;

        _cache[key] = (serialized, expiresAt);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix)
    {
        var keysToRemove = _cache.Keys.Where(k => k.StartsWith(prefix)).ToList();
        foreach (var key in keysToRemove)
            _cache.TryRemove(key, out _);

        return Task.CompletedTask;
    }
}

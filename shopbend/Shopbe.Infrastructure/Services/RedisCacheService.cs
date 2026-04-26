// Infrastructure/Services/RedisCacheService.cs

using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.Infrastructure.Services;

public class RedisCacheService(IDistributedCache cache) : ICacheService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<T?> GetAsync<T>(string key)
    {
        var data = await cache.GetStringAsync(key);
        if (data is null) return default;
        return JsonSerializer.Deserialize<T>(data, _jsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(10)
        };
        var data = JsonSerializer.Serialize(value, _jsonOptions);
        await cache.SetStringAsync(key, data, options);
    }

    public async Task RemoveAsync(string key)
    {
        await cache.RemoveAsync(key);
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        // dùng cho invalidate hàng loạt, ví dụ xóa cache "products:*"
        // cần IConnectionMultiplexer để scan keys
        throw new NotImplementedException("Cần inject IConnectionMultiplexer");
    }
}
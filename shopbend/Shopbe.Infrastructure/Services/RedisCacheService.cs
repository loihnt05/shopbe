// Infrastructure/Services/RedisCacheService.cs

using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Shopbe.Application.Common.Interfaces;
using StackExchange.Redis;

namespace Shopbe.Infrastructure.Services;

public class RedisCacheService(IDistributedCache cache, IConnectionMultiplexer redis, IConfiguration configuration) : ICacheService
{
    private readonly string _instanceName = configuration["Redis:InstanceName"] ?? "shopbe:";
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
        var pattern = $"{_instanceName}{prefix}*";
        var keys = new HashSet<RedisKey>();

        foreach (var endPoint in redis.GetEndPoints())
        {
            var server = redis.GetServer(endPoint);
            if (!server.IsConnected)
            {
                continue;
            }

            foreach (var key in server.Keys(pattern: pattern))
            {
                keys.Add(key);
            }
        }

        if (keys.Count == 0)
        {
            return;
        }

        await redis.GetDatabase().KeyDeleteAsync(keys.ToArray());
    }
}

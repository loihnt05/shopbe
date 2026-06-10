using System.Collections.Concurrent;
using System.Text.Json;
using Shopbe.Application.Common.Interfaces;

namespace Shopbe.E2E.Tests.Fakes;

public sealed class FakeCacheService : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ConcurrentDictionary<string, CacheEntry> _entries = new();

    public Task<T?> GetAsync<T>(string key)
    {
        if (!_entries.TryGetValue(key, out var entry))
        {
            return Task.FromResult<T?>(default);
        }

        if (entry.ExpiresAtUtc is { } expiresAtUtc && expiresAtUtc <= DateTimeOffset.UtcNow)
        {
            _entries.TryRemove(key, out _);
            return Task.FromResult<T?>(default);
        }

        return Task.FromResult(JsonSerializer.Deserialize<T>(entry.Json, JsonOptions));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var expiresAtUtc = DateTimeOffset.UtcNow.Add(expiry ?? TimeSpan.FromMinutes(10));
        var json = JsonSerializer.Serialize(value, JsonOptions);
        _entries[key] = new CacheEntry(json, expiresAtUtc);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _entries.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix)
    {
        foreach (var key in _entries.Keys.Where(key => key.StartsWith(prefix, StringComparison.Ordinal)))
        {
            _entries.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    private sealed record CacheEntry(string Json, DateTimeOffset? ExpiresAtUtc);
}

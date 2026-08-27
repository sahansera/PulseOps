using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace PulseOps.Api;

public sealed class ServiceStatusCache(IDistributedCache cache)
{
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromSeconds(30);

    public async Task<ServiceStatus?> GetAsync(
        string serviceId,
        CancellationToken cancellationToken)
    {
        var value = await cache.GetAsync(GetKey(serviceId), cancellationToken);

        return value is null
            ? null
            : JsonSerializer.Deserialize<ServiceStatus>(value);
    }

    public Task SetAsync(
        string serviceId,
        ServiceStatus status,
        CancellationToken cancellationToken)
    {
        var value = JsonSerializer.SerializeToUtf8Bytes(status);

        return cache.SetAsync(
            GetKey(serviceId),
            value,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheLifetime
            },
            cancellationToken);
    }

    private static string GetKey(string serviceId) =>
        $"service-status:{serviceId.ToLowerInvariant()}";
}

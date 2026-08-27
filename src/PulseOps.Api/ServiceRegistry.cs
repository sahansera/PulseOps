using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace PulseOps.Api;

public sealed record RegisterServiceRequest(string Id, string Name, string Url);

public sealed record UpdateServiceStatusRequest(string Status);

public sealed record ServiceDefinition(string Id, string Name, string Url)
{
    public MonitoredService ToResponse(ServiceStatus status) =>
        new(Id, Name, Url, status);
}

public sealed record MonitoredService(string Id, string Name, string Url, ServiceStatus Status);

public enum ServiceStatus
{
    Unknown,
    Healthy,
    Unhealthy
}

public sealed class ServiceRegistry
{
    private readonly ConcurrentDictionary<string, ServiceDefinition> _services =
        new(StringComparer.OrdinalIgnoreCase);

    public ServiceRegistry()
    {
        TryAdd(new ServiceDefinition(
            "payments-api",
            "Payments API",
            "https://example.com/health"));
    }

    public IReadOnlyCollection<ServiceDefinition> GetAll() =>
        _services.Values.OrderBy(service => service.Name).ToArray();

    public bool TryGet(string id, [NotNullWhen(true)] out ServiceDefinition? service) =>
        _services.TryGetValue(id, out service);

    public bool TryAdd(ServiceDefinition service) =>
        _services.TryAdd(service.Id, service);
}

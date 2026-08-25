using System.Collections.Concurrent;
using PulseOps.Api;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddSingleton<ServiceRegistry>();

var app = builder.Build();

var services = app.MapGroup("/services");

services.MapGet("", (ServiceRegistry registry) => Results.Ok(registry.GetAll()));

services.MapGet("/{id}", (string id, ServiceRegistry registry) =>
    registry.TryGet(id, out var service)
        ? Results.Ok(service)
        : Results.NotFound());

services.MapPost("", (RegisterServiceRequest request, ServiceRegistry registry) =>
{
    if (string.IsNullOrWhiteSpace(request.Id) ||
        string.IsNullOrWhiteSpace(request.Name) ||
        !Uri.TryCreate(request.Url, UriKind.Absolute, out _))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["service"] = ["Id, name, and a valid absolute URL are required."]
        });
    }

    var service = new MonitoredService(
        request.Id.Trim(),
        request.Name.Trim(),
        request.Url.Trim(),
        ServiceStatus.Unknown);

    return registry.TryAdd(service)
        ? Results.Created($"/services/{service.Id}", service)
        : Results.Conflict(new { message = $"A service with id '{service.Id}' already exists." });
});

app.MapDefaultEndpoints();
app.Run();

namespace PulseOps.Api
{
    public sealed record RegisterServiceRequest(string Id, string Name, string Url);

    public sealed record MonitoredService(string Id, string Name, string Url, ServiceStatus Status);

    public enum ServiceStatus
    {
        Unknown,
        Healthy,
        Unhealthy
    }

    public sealed class ServiceRegistry
    {
        private readonly ConcurrentDictionary<string, MonitoredService> _services =
            new(StringComparer.OrdinalIgnoreCase);

        public ServiceRegistry()
        {
            TryAdd(new MonitoredService(
                "payments-api",
                "Payments API",
                "https://example.com/health",
                ServiceStatus.Unknown));
        }

        public IReadOnlyCollection<MonitoredService> GetAll() =>
            _services.Values.OrderBy(service => service.Name).ToArray();

        public bool TryGet(string id, out MonitoredService? service) =>
            _services.TryGetValue(id, out service);

        public bool TryAdd(MonitoredService service) =>
            _services.TryAdd(service.Id, service);
    }
}

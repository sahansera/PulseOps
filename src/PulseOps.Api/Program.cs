using System.Text.Json.Serialization;
using PulseOps.Api;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddRedisDistributedCache("cache");

builder.Services.AddSingleton<ServiceRegistry>();
builder.Services.AddSingleton<ServiceStatusCache>();
builder.Services.AddSingleton<ApiInstance>();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

var services = app.MapGroup("/services");

services.MapGet("", async (
    ServiceRegistry registry,
    ServiceStatusCache statusCache,
    CancellationToken cancellationToken) =>
{
    var responses = await Task.WhenAll(registry.GetAll().Select(async service =>
        service.ToResponse(await statusCache.GetAsync(service.Id, cancellationToken)
            ?? ServiceStatus.Unknown)));

    return Results.Ok(responses);
});

services.MapGet("/{id}", async Task<IResult> (
    string id,
    ServiceRegistry registry,
    ServiceStatusCache statusCache,
    CancellationToken cancellationToken) =>
{
    if (!registry.TryGet(id, out var service))
    {
        return Results.NotFound();
    }

    var status = await statusCache.GetAsync(service.Id, cancellationToken)
        ?? ServiceStatus.Unknown;

    return Results.Ok(service.ToResponse(status));
});

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

    var service = new ServiceDefinition(
        request.Id.Trim(),
        request.Name.Trim(),
        request.Url.Trim());

    return registry.TryAdd(service)
        ? Results.Created($"/services/{service.Id}", service.ToResponse(ServiceStatus.Unknown))
        : Results.Conflict(new { message = $"A service with id '{service.Id}' already exists." });
});

services.MapPut("/{id}/status", async Task<IResult> (
    string id,
    UpdateServiceStatusRequest request,
    ServiceRegistry registry,
    ServiceStatusCache statusCache,
    CancellationToken cancellationToken) =>
{
    if (!registry.TryGet(id, out var service))
    {
        return Results.NotFound();
    }

    if (!Enum.TryParse<ServiceStatus>(request.Status, true, out var status) ||
        status is ServiceStatus.Unknown)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["status"] = ["Status must be Healthy or Unhealthy."]
        });
    }

    await statusCache.SetAsync(service.Id, status, cancellationToken);

    return Results.Ok(service.ToResponse(status));
});

app.MapGet("/diagnostics/instance", (ApiInstance instance) => Results.Ok(instance));

app.MapDefaultEndpoints();
app.Run();

namespace PulseOps.Api
{
    public sealed record ApiInstance(Guid InstanceId)
    {
        public ApiInstance() : this(Guid.NewGuid())
        {
        }
    }
}

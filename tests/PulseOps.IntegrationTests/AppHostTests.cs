using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace PulseOps.IntegrationTests;

public sealed class AppHostTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(2);

    [Fact]
    public async Task WebResourceStartsAndReturnsOk()
    {
        using var cts = new CancellationTokenSource(DefaultTimeout);
        var cancellationToken = cts.Token;

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.PulseOps_AppHost>(cancellationToken);

        await using var app = await appHost.BuildAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        await app.StartAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        await app.ResourceNotifications.WaitForResourceHealthyAsync("web", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        using var client = app.CreateHttpClient("web");
        using var response = await client.GetAsync("/", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ApiStartsWithSeedService()
    {
        using var cts = new CancellationTokenSource(DefaultTimeout);
        var cancellationToken = cts.Token;

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.PulseOps_AppHost>(cancellationToken);

        await using var app = await appHost.BuildAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        await app.StartAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        await app.ResourceNotifications.WaitForResourceHealthyAsync("api", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        using var client = app.CreateHttpClient("api");
        using var response = await client.GetAsync("/services", cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("payments-api", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ServiceStatusIsSharedAcrossApiReplicas()
    {
        using var cts = new CancellationTokenSource(DefaultTimeout);
        var cancellationToken = cts.Token;

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.PulseOps_AppHost>(cancellationToken);

        await using var app = await appHost.BuildAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        await app.StartAsync(cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        await app.ResourceNotifications.WaitForResourceHealthyAsync("cache", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);

        var replicaEndpoints = await WaitForHealthyApiReplicasAsync(app, cancellationToken);

        Assert.Equal(2, replicaEndpoints.Count);
        Assert.NotEqual(replicaEndpoints[0], replicaEndpoints[1]);

        using var clientA = new HttpClient { BaseAddress = replicaEndpoints[0] };
        using var clientB = new HttpClient { BaseAddress = replicaEndpoints[1] };

        var instanceA = await GetInstanceIdAsync(clientA, cancellationToken);
        var instanceB = await GetInstanceIdAsync(clientB, cancellationToken);

        Assert.NotEqual(instanceA, instanceB);

        using var writeResponse = await clientA.PutAsJsonAsync(
            "/services/payments-api/status",
            new { status = "Healthy" },
            cancellationToken);

        Assert.Equal(HttpStatusCode.OK, writeResponse.StatusCode);

        using var readResponse = await clientB.GetAsync(
            "/services/payments-api",
            cancellationToken);

        Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);

        using var service = JsonDocument.Parse(
            await readResponse.Content.ReadAsStreamAsync(cancellationToken));

        Assert.Equal("Healthy", service.RootElement.GetProperty("status").GetString());
    }

    private static async Task<IReadOnlyList<Uri>> WaitForHealthyApiReplicasAsync(
        DistributedApplication app,
        CancellationToken cancellationToken)
    {
        var healthyReplicaIds = new HashSet<string>(StringComparer.Ordinal);

        await foreach (var resourceEvent in app.ResourceNotifications.WatchAsync(cancellationToken))
        {
            if (!string.Equals(resourceEvent.Resource.Name, "api", StringComparison.Ordinal) ||
                resourceEvent.Snapshot.HealthStatus is not HealthStatus.Healthy)
            {
                continue;
            }

            healthyReplicaIds.Add(resourceEvent.ResourceId);

            if (healthyReplicaIds.Count == 2)
            {
                var resourceLogs = app.Services.GetRequiredService<ResourceLoggerService>();
                var endpoints = new List<Uri>(2);

                foreach (var replicaId in healthyReplicaIds)
                {
                    endpoints.Add(await GetListeningEndpointAsync(resourceLogs, replicaId));
                }

                return endpoints;
            }
        }

        throw new InvalidOperationException("The two API replicas did not become healthy.");
    }

    private static async Task<Uri> GetListeningEndpointAsync(
        ResourceLoggerService resourceLogs,
        string replicaId)
    {
        const string marker = "Now listening on:";

        await foreach (var logBatch in resourceLogs.GetAllAsync(replicaId))
        {
            foreach (var logLine in logBatch)
            {
                var markerIndex = logLine.Content.IndexOf(marker, StringComparison.Ordinal);
                if (markerIndex < 0)
                {
                    continue;
                }

                var address = logLine.Content[(markerIndex + marker.Length)..].Trim();
                if (Uri.TryCreate(address, UriKind.Absolute, out var endpoint))
                {
                    return endpoint;
                }
            }
        }

        throw new InvalidOperationException(
            $"API replica '{replicaId}' did not report its listening endpoint.");
    }

    private static async Task<Guid> GetInstanceIdAsync(
        HttpClient client,
        CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync("/diagnostics/instance", cancellationToken);
        response.EnsureSuccessStatusCode();

        using var instance = JsonDocument.Parse(
            await response.Content.ReadAsStreamAsync(cancellationToken));

        return instance.RootElement.GetProperty("instanceId").GetGuid();
    }
}

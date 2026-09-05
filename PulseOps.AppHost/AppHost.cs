var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var postgres = builder.AddPostgres("postgres");

if (!string.Equals(
    builder.Configuration["PulseOps:UsePostgresDataVolume"],
    "false",
    StringComparison.OrdinalIgnoreCase))
{
    postgres.WithDataVolume();
}

var database = postgres.AddDatabase("pulseops");

var migrations = builder.AddProject<Projects.PulseOps_Migrations>("migrations")
    .WithReference(database)
    .WaitFor(database);

var api = builder.AddProject<Projects.PulseOps_Api>("api")
    .WithHttpEndpoint()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(database)
    .WaitFor(database)
    .WaitForCompletion(migrations)
    .WithReplicas(2);

builder.AddProject<Projects.PulseOps_Web>("web")
    .WithHttpEndpoint()
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.PulseOps_Api>("api")
    .WithHttpEndpoint()
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.PulseOps_Web>("web")
    .WithHttpEndpoint()
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();

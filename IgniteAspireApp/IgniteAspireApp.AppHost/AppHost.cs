using System.Net.Sockets;

var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.IgniteAspireApp_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.IgniteAspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

// TODO: WithVolume?
builder.AddContainer("apacheignite", "apacheignite/ignite:3.1.0")
    .WithContainerName("ignite")
    .WithEndpoint(targetPort: 10300, port: 10300, name: "ignite-rest-api", scheme: "http")
    .WithEndpoint(targetPort: 10800, port: 10800, name: "ignite-client", scheme: "tcp", protocol: ProtocolType.Tcp);

builder.Build().Run();

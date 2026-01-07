using System.Net.Sockets;

var builder = DistributedApplication.CreateBuilder(args);

var igniteService = builder.AddContainer("apacheignite", "apacheignite/ignite:3.1.0")
    .WithContainerName("ignite")
    .WithEndpoint(targetPort: 10300, port: 10300, name: "ignite-rest-api", scheme: "http")
    .WithEndpoint(targetPort: 10800, port: 10800, name: "ignite-client", scheme: "tcp", protocol: ProtocolType.Tcp)
    .WithBindMount("./ignite/node-config.json", "/opt/ignite/etc/node-config.json")
    .WithHttpHealthCheck("/", 409, "ignite-rest-api");

const string curlCmd =
    """
    curl -i --request POST --header "Content-Type: application/json" --data '{"metaStorageNodes": ["defaultNode"], "clusterName": "myCluster"}' ${APACHEIGNITE_IGNITE_REST_API}/management/v1/cluster/init
    """;

builder.AddContainer("apacheignite-init", "curlimages/curl")
    .WithReference(igniteService.GetEndpoint("ignite-rest-api"))
    .WaitFor(igniteService)
    .WithArgs("sh", "-c", curlCmd)
    .WithLifetime(ContainerLifetime.Session);

var apiService = builder.AddProject<Projects.IgniteAspireApp_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(igniteService.GetEndpoint("ignite-client"))
    .WaitFor(igniteService);

builder.AddProject<Projects.IgniteAspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();

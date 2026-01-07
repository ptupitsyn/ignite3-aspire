using Apache.Ignite;
using Apache.Ignite.Network;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Ignite.
var clientUrl = Environment.GetEnvironmentVariable("APACHEIGNITE_IGNITE_CLIENT")
                ?? throw new InvalidOperationException("APACHEIGNITE_IGNITE_CLIENT is not set");

clientUrl = clientUrl.Replace("tcp://", string.Empty); // TODO: Can we fix this in Aspire?

Console.WriteLine($"Ignite client url: {clientUrl}");

builder.Services.AddIgniteClientGroup(services => new IgniteClientGroupConfiguration
{
    ClientConfiguration = new IgniteClientConfiguration(clientUrl)
    {
        LoggerFactory = services.GetRequiredService<ILoggerFactory>()
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => "API service is running. Navigate to /nodes to see Ignite nodes.");

app.MapGet("/nodes", async ([FromServices] IgniteClientGroup igniteGrp) =>
{
    IIgnite ignite = await igniteGrp.GetIgniteAsync();

    IList<IClusterNode> nodes = await ignite.GetClusterNodesAsync();

    IgniteNode[] vms = nodes
        .Select(x => new IgniteNode(x.Name, x.Id.ToString(), x.Address.ToString()!))
        .ToArray();

    return vms;
})
.WithName("GetClusterNodes");

app.MapDefaultEndpoints();

app.Run();

record IgniteNode(string Name, string Id, string Address);

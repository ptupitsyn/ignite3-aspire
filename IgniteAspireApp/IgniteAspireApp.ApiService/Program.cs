using Apache.Ignite;
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

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/", () => "API service is running. Navigate to /weatherforecast to see sample data.");

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/nodes", async ([FromServices] IgniteClientGroup igniteGrp) =>
{
    IIgnite ignite = await igniteGrp.GetIgniteAsync();

    return await ignite.GetClusterNodesAsync();
});

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

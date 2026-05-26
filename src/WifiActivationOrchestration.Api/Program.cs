using Microsoft.Extensions.Options;
using WifiActivationOrchestration.Api.Application.Services;
using WifiActivationOrchestration.Api.Infrastructure.Configuration;
using WifiActivationOrchestration.Api.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services
    .AddOptions<NetworkApiOptions>()
    .Bind(builder.Configuration.GetRequiredSection(NetworkApiOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient<INetworkInfrastructureClient, NetworkInfrastructureClient>(
    (serviceProvider, httpClient) =>
    {
        var options = serviceProvider
            .GetRequiredService<IOptions<NetworkApiOptions>>()
            .Value;

        httpClient.BaseAddress = new Uri(options.InfrastructureBaseUrl);
        httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    });

builder.Services.AddHttpClient<INetworkActivationService, NetworkActivationService>(
    (serviceProvider, httpClient) =>
    {
        var options = serviceProvider
            .GetRequiredService<IOptions<NetworkApiOptions>>()
            .Value;

        httpClient.BaseAddress = new Uri(options.ControllerBaseUrl);
        httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    });

builder.Services.AddScoped<IWifiActivationService, WifiActivationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public partial class Program;
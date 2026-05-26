using Microsoft.Extensions.Options;
using WifiActivationOrchestration.Api.Infrastructure.Configuration;
using WifiActivationOrchestration.Api.Infrastructure.Models;

namespace WifiActivationOrchestration.Api.Infrastructure.Services;

public sealed class NetworkInfrastructureClient : INetworkInfrastructureClient
{
    private readonly HttpClient _httpClient;
    private readonly NetworkApiOptions _options;
    private readonly ILogger<NetworkInfrastructureClient> _logger;

    public NetworkInfrastructureClient(
        HttpClient httpClient,
        IOptions<NetworkApiOptions> options,
        ILogger<NetworkInfrastructureClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<NetworkInfrastructureResponse> GetSpeedProfilesAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving speed profiles from Network Infrastructure API.");

        using var response = await _httpClient.GetAsync(
            _options.SpeedProfilesPath,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Network Infrastructure API returned status code {(int)response.StatusCode}.",
                inner: null,
                statusCode: response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<NetworkInfrastructureResponse>(
            cancellationToken);

        return result ?? new NetworkInfrastructureResponse();
    }
}
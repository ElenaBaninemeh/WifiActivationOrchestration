using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using WifiActivationOrchestration.Api.Configuration;
using WifiActivationOrchestration.Api.Models.External;

namespace WifiActivationOrchestration.Api.Services.External;

public sealed class NetworkControllerClient : INetworkControllerClient
{
    private readonly HttpClient _httpClient;
    private readonly NetworkApiOptions _options;
    private readonly ILogger<NetworkControllerClient> _logger;

    public NetworkControllerClient(
        HttpClient httpClient,
        IOptions<NetworkApiOptions> options,
        ILogger<NetworkControllerClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task ActivateWifiAsync(
        NetworkControllerActivationRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Sending WiFi activation request to Network Controller for customer {CustomerId}.",
            request.CustomerId);

        using var response = await _httpClient.PostAsJsonAsync(
            _options.ActivationPath,
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Network Controller API returned status code {(int)response.StatusCode}.",
                inner: null,
                statusCode: response.StatusCode);
        }
    }
}
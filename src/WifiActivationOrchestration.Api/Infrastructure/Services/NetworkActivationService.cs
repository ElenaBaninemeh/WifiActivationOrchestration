using Microsoft.Extensions.Options;
using WifiActivationOrchestration.Api.Infrastructure.Configuration;
using WifiActivationOrchestration.Api.Infrastructure.Models;

namespace WifiActivationOrchestration.Api.Infrastructure.Services;

/// <summary>
/// Sends WiFi activation requests to the external Network Controller API.
/// </summary>
public sealed class NetworkActivationService : INetworkActivationService
{
    private readonly HttpClient _httpClient;
    private readonly NetworkApiOptions _options;
    private readonly ILogger<NetworkActivationService> _logger;

    public NetworkActivationService(
        HttpClient httpClient,
        IOptions<NetworkApiOptions> options,
        ILogger<NetworkActivationService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task ActivateWifiAsync(NetworkActivationRequest request, CancellationToken cancellationToken)
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
using Microsoft.Extensions.Options;
using WifiActivationOrchestration.Api.Infrastructure.Configuration;
using WifiActivationOrchestration.Api.Infrastructure.Models;

namespace WifiActivationOrchestration.Api.Infrastructure.Services;

/// <summary>
/// Retrieves speed profile data from the external Network Infrastructure API.
/// </summary>
public sealed class SpeedProfileService : ISpeedProfileService
{
    private readonly HttpClient _httpClient;
    private readonly NetworkApiOptions _options;
    private readonly ILogger<SpeedProfileService> _logger;

    public SpeedProfileService(
        HttpClient httpClient,
        IOptions<NetworkApiOptions> options,
        ILogger<SpeedProfileService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<SpeedProfileResponse> GetSpeedProfilesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving speed profiles from Network Infrastructure API.");

        using var response = await _httpClient.GetAsync(_options.SpeedProfilesPath, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Network Infrastructure API returned status code {(int)response.StatusCode}.",
                inner: null,
                statusCode: response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<SpeedProfileResponse>(
            cancellationToken);

        return result ?? new SpeedProfileResponse();
    }
}
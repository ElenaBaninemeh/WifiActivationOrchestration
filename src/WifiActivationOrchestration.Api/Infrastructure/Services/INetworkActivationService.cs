using WifiActivationOrchestration.Api.Infrastructure.Models;

namespace WifiActivationOrchestration.Api.Infrastructure.Services;

/// <summary>
/// Defines the external network activation operation.
/// </summary>
public interface INetworkActivationService
{
    /// <summary>
    /// Sends a WiFi activation request to the external Network Controller API.
    /// </summary>
    /// <param name="request">
    /// Network activation payload containing the customer and speed profile data.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the external API request.
    /// </param>
    Task ActivateWifiAsync( NetworkActivationRequest request, CancellationToken cancellationToken);
}
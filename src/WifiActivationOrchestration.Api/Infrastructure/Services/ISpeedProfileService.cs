using WifiActivationOrchestration.Api.Infrastructure.Models;

namespace WifiActivationOrchestration.Api.Infrastructure.Services;

/// <summary>
/// Defines access to available network speed profiles.
/// </summary>
public interface ISpeedProfileService
{
    /// <summary>
    /// Retrieves the available speed profiles from the Network Infrastructure API.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the external API request.
    /// </param>
    /// <returns>
    /// A response containing the available speed profiles.
    /// </returns>
    Task<SpeedProfileResponse> GetSpeedProfilesAsync(CancellationToken cancellationToken);
}
namespace WifiActivationOrchestration.Api.Infrastructure.Models;

/// <summary>
/// Response returned by the external Network Infrastructure API containing
/// the available speed profiles.
/// </summary>
public sealed class SpeedProfileResponse
{
    /// <summary>
    /// Gets or sets the external request identifier returned by the Network Infrastructure API.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Gets or sets the available speed profiles.
    /// </summary>
    public List<SpeedProfile> SpeedProfiles { get; set; } = [];
}

/// <summary>
/// Represents a speed profile available in the Network Infrastructure API.
/// </summary>
public sealed class SpeedProfile
{
    /// <summary>
    /// Gets or sets the speed profile code, for example <c>SP-500</c>.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the download speed in Mbps.
    /// </summary>
    public int DownloadSpeedMbps { get; set; }

    /// <summary>
    /// Gets or sets the upload speed in Mbps.
    /// </summary>
    public int UploadSpeedMbps { get; set; }
}
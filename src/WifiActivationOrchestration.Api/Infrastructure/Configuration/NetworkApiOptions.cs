using System.ComponentModel.DataAnnotations;

namespace WifiActivationOrchestration.Api.Infrastructure.Configuration;

/// <summary>
/// Configuration options for the external network APIs used by the WiFi activation flow.
/// </summary>
public sealed class NetworkApiOptions
{
    /// <summary>
    /// Configuration section name used to bind these options.
    /// </summary>
    public const string SectionName = "NetworkApis";

    /// <summary>
    /// Gets or sets the base URL of the Network Infrastructure API.
    /// </summary>
    /// <remarks>
    /// In local development this points to the WireMock server.
    /// </remarks>
    [Required]
    [Url]
    public string InfrastructureBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL of the Network Controller API.
    /// </summary>
    [Required]
    [Url]
    public string ControllerBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the relative path used to retrieve available speed profiles.
    /// </summary>
    [Required]
    public string SpeedProfilesPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the relative path used to send WiFi activation requests.
    /// </summary>
    [Required]
    public string ActivationPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timeout, in seconds, for external network API calls.
    /// </summary>
    [Range(1, 60)]
    public int TimeoutSeconds { get; set; } = 10;
}
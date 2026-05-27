namespace WifiActivationOrchestration.Api.Infrastructure.Models;

/// <summary>
/// Request payload sent to the external Network Controller API to activate WiFi.
/// </summary>
public sealed class NetworkActivationRequest
{
    /// <summary>
    /// Gets or sets the customer identifier.
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer installation address.
    /// </summary>
    public string CustomerAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the resolved upstream speed in Mbps.
    /// </summary>
    public int UpstreamSpeed { get; set; }

    /// <summary>
    /// Gets or sets the resolved downstream speed in Mbps.
    /// </summary>
    public int DownstreamSpeed { get; set; }
}
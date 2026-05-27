namespace WifiActivationOrchestration.Api.Contracts.Responses;

/// <summary>
/// Response returned by the API when a WiFi activation request is accepted.
/// </summary>
/// <remarks>
/// This response represents the API result returned to the caller. It is different
/// from the payload sent to the external Network Controller API, which only contains
/// customer address and resolved speed values.
/// </remarks>
public sealed record WifiActivationResponse
{
    /// <summary>
    /// Gets the external activation/order identifier received from the customer portal.
    /// </summary>
    public string? ExternalId { get; init; }

    /// <summary>
    /// Gets the customer identifier used for the WiFi activation.
    /// </summary>
    public string CustomerId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the customer installation address.
    /// </summary>
    public string CustomerAddress { get; init; } = string.Empty;

    /// <summary>
    /// Gets the requested speed profile code.
    /// </summary>
    public string SpeedProfile { get; init; } = string.Empty;

    /// <summary>
    /// Gets the resolved upstream speed in Mbps.
    /// </summary>
    public int UpstreamSpeed { get; init; }

    /// <summary>
    /// Gets the resolved downstream speed in Mbps.
    /// </summary>
    public int DownstreamSpeed { get; init; }

    /// <summary>
    /// Gets the activation status returned to the API caller.
    /// </summary>
    /// <remarks>
    /// The service sets this to <c>accepted</c> after the activation payload has
    /// been sent successfully to the external Network Controller API.
    /// </remarks>
    public string Status { get; init; } = string.Empty;
}
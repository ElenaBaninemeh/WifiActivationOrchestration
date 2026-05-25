namespace WifiActivationOrchestration.Api.Models;

public sealed record WifiActivationResponse
{
    public string? ExternalId { get; init; }

    public string CustomerId { get; init; } = string.Empty;

    public string CustomerAddress { get; init; } = string.Empty;

    public string SpeedProfile { get; init; } = string.Empty;

    public int UpstreamSpeed { get; init; }

    public int DownstreamSpeed { get; init; }

    public string Status { get; init; } = "accepted";
}
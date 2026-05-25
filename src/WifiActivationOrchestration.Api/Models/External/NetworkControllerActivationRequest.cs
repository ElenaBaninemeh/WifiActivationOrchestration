namespace WifiActivationOrchestration.Api.Models.External;

public sealed class NetworkControllerActivationRequest
{
    public string CustomerId { get; set; } = string.Empty;

    public string CustomerAddress { get; set; } = string.Empty;

    public int UpstreamSpeed { get; set; }

    public int DownstreamSpeed { get; set; }
}
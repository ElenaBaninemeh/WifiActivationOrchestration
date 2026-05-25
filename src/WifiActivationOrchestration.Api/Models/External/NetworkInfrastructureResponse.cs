namespace WifiActivationOrchestration.Api.Models.External;

public sealed class NetworkInfrastructureResponse
{
    public string? RequestId { get; set; }

    public List<SpeedProfile> SpeedProfiles { get; set; } = [];
}

public sealed class SpeedProfile
{
    public string Code { get; set; } = string.Empty;

    public int DownloadSpeedMbps { get; set; }

    public int UploadSpeedMbps { get; set; }
}
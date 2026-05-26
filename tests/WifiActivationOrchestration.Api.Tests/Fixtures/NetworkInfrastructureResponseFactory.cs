using WifiActivationOrchestration.Api.Models.External;

namespace WifiActivationOrchestration.Api.Tests.TestData;

public static class NetworkInfrastructureResponseFactory
{
    public static NetworkInfrastructureResponse WithSpeedProfile(
        string code = "SP-500",
        int downloadSpeedMbps = 500,
        int uploadSpeedMbps = 100)
    {
        return new NetworkInfrastructureResponse
        {
            RequestId = "REQ-001",
            SpeedProfiles =
            [
                new SpeedProfile
                {
                    Code = code,
                    DownloadSpeedMbps = downloadSpeedMbps,
                    UploadSpeedMbps = uploadSpeedMbps
                }
            ]
        };
    }
}
using WifiActivationOrchestration.Api.Infrastructure.Models;

namespace WifiActivationOrchestration.Api.Tests.Fixtures;

public static class SpeedProfileResponseFactory
{
    public static SpeedProfileResponse WithSpeedProfile(string code = "SP-500", int downloadSpeedMbps = 500, int uploadSpeedMbps = 100)
    {
        return new SpeedProfileResponse
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
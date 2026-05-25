using WifiActivationOrchestration.Api.Models.External;

namespace WifiActivationOrchestration.Api.Services.External;

public interface INetworkInfrastructureClient
{
    Task<NetworkInfrastructureResponse> GetSpeedProfilesAsync(
        CancellationToken cancellationToken);
}
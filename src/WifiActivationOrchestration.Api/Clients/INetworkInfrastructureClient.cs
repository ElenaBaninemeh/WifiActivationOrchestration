using WifiActivationOrchestration.Api.Models;

namespace WifiActivationOrchestration.Api.Clients;

public interface INetworkInfrastructureClient
{
    Task<NetworkInfrastructureResponse> GetSpeedProfilesAsync(
        CancellationToken cancellationToken);
}
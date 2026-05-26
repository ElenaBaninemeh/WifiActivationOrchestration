using WifiActivationOrchestration.Api.Infrastructure.Models;

namespace WifiActivationOrchestration.Api.Infrastructure.Services;

public interface INetworkInfrastructureClient
{
    Task<NetworkInfrastructureResponse> GetSpeedProfilesAsync(
        CancellationToken cancellationToken);
}
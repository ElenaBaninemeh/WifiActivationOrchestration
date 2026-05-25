using WifiActivationOrchestration.Api.Models;

namespace WifiActivationOrchestration.Api.Clients;

public interface INetworkControllerClient
{
    Task ActivateWifiAsync(
        NetworkControllerActivationRequest request,
        CancellationToken cancellationToken);
}
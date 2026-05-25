using WifiActivationOrchestration.Api.Models.External;

namespace WifiActivationOrchestration.Api.Services.External;

public interface INetworkControllerClient
{
    Task ActivateWifiAsync(
        NetworkControllerActivationRequest request,
        CancellationToken cancellationToken);
}
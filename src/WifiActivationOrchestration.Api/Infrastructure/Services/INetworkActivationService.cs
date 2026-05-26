using WifiActivationOrchestration.Api.Infrastructure.Models;

namespace WifiActivationOrchestration.Api.Infrastructure.Services;

public interface INetworkActivationService
{
    Task ActivateWifiAsync(
        NetworkControllerActivationRequest request,
        CancellationToken cancellationToken);
}
using WifiActivationOrchestration.Api.Models;

namespace WifiActivationOrchestration.Api.Services;

public interface IWifiActivationService
{
    Task<WifiActivationResponse> ActivateAsync(
        CustomerActivationRequest request,
        CancellationToken cancellationToken);
}
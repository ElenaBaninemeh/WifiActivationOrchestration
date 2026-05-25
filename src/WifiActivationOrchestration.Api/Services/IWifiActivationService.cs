using WifiActivationOrchestration.Api.Models.Requests;
using WifiActivationOrchestration.Api.Models.Results;

namespace WifiActivationOrchestration.Api.Services;

public interface IWifiActivationService
{
    Task<WifiActivationResult> ActivateAsync(
        CustomerActivationRequest request,
        CancellationToken cancellationToken);
}
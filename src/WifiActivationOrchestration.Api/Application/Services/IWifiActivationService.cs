using WifiActivationOrchestration.Api.Application.Common;
using WifiActivationOrchestration.Api.Contracts.Requests;

namespace WifiActivationOrchestration.Api.Application.Services;

public interface IWifiActivationService
{
    Task<WifiActivationResult> ActivateAsync(
        CustomerActivationRequest request,
        CancellationToken cancellationToken);
}
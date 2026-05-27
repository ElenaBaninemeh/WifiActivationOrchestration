using WifiActivationOrchestration.Api.Application.Common;
using WifiActivationOrchestration.Api.Contracts.Requests;

namespace WifiActivationOrchestration.Api.Application.Services;

public interface IWifiActivationService
{
    /// <summary>
    /// Activates WiFi for the customer activation order.
    /// </summary>
    /// <param name="request">Customer activation request received by the API.</param>
    /// <param name="cancellationToken">Token used to cancel the activation flow.</param>
    /// <returns>
    /// An application-level result describing whether the activation was accepted
    /// or why it could not be completed.
    /// </returns>
    Task<WifiActivationResult> ActivateAsync(CustomerActivationRequest request, CancellationToken cancellationToken);
}
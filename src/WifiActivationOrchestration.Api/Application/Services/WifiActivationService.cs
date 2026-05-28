using System.Text.Json;
using WifiActivationOrchestration.Api.Application.Common;
using WifiActivationOrchestration.Api.Application.Mapping;
using WifiActivationOrchestration.Api.Contracts.Requests;
using WifiActivationOrchestration.Api.Contracts.Responses;
using WifiActivationOrchestration.Api.Infrastructure.Models;
using WifiActivationOrchestration.Api.Infrastructure.Services;

namespace WifiActivationOrchestration.Api.Application.Services;

/// <summary>
/// Orchestrates the WiFi activation use case.
/// </summary>
public sealed class WifiActivationService : IWifiActivationService
{
    private readonly ISpeedProfileService _speedProfileService;
    private readonly INetworkActivationService _networkActivationService;
    private readonly ILogger<WifiActivationService> _logger;

    public WifiActivationService(
        ISpeedProfileService speedProfileService,
        INetworkActivationService networkActivationService,
        ILogger<WifiActivationService> logger)
    {
        _speedProfileService = speedProfileService;
        _networkActivationService = networkActivationService;
        _logger = logger;
    }

    ///<inheritdoc/>
    public async Task<WifiActivationResult> ActivateAsync(CustomerActivationRequest request, CancellationToken cancellationToken)
    {
        var activationInput = CustomerActivationRequestMapper.MapToActivationInput(request);

        if (activationInput is null)
        {
            return WifiActivationResult.InvalidRequest(
                "Request is missing externalId, customerId, customerAddress, or speedProfile.");
        }

        try
        {
            var infrastructureResponse = await _speedProfileService.GetSpeedProfilesAsync(cancellationToken);

            var speedProfile = infrastructureResponse.SpeedProfiles.FirstOrDefault(profile =>
                string.Equals(
                    profile.Code,
                    activationInput.SpeedProfile,
                    StringComparison.OrdinalIgnoreCase));

            if (speedProfile is null)
            {
                return WifiActivationResult.SpeedProfileNotFound(
                    $"Speed profile '{activationInput.SpeedProfile}' was not found.");
            }

            var controllerRequest = new NetworkActivationRequest
            {
                CustomerId = activationInput.CustomerId,
                CustomerAddress = activationInput.CustomerAddress,
                UpstreamSpeed = speedProfile.UploadSpeedMbps,
                DownstreamSpeed = speedProfile.DownloadSpeedMbps
            };

            await _networkActivationService.ActivateWifiAsync(
                controllerRequest,
                cancellationToken);

            var response = new WifiActivationResponse
            {
                ExternalId = activationInput.ExternalId,
                CustomerId = activationInput.CustomerId,
                CustomerAddress = activationInput.CustomerAddress,
                SpeedProfile = speedProfile.Code,
                UpstreamSpeed = speedProfile.UploadSpeedMbps,
                DownstreamSpeed = speedProfile.DownloadSpeedMbps,
                Status = "accepted"
            };

            return WifiActivationResult.Accepted(response);
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(
                exception,
                "WiFi activation failed because an external network API returned invalid JSON for customer {CustomerId}.",
                activationInput.CustomerId);

            return WifiActivationResult.DependencyFailure(
                "An external network API returned an invalid response.");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "WiFi activation timed out for customer {CustomerId}.",
                activationInput.CustomerId);

            return WifiActivationResult.DependencyTimeout(
                "A timeout occurred while communicating with an external network API.");
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(
                exception,
                "WiFi activation failed while communicating with an external network API for customer {CustomerId}.",
                activationInput.CustomerId);

            return WifiActivationResult.DependencyFailure(
                "Failed to communicate with an external network API.");
        }
    }
}
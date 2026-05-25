using System.Text.Json;
using WifiActivationOrchestration.Api.Mapping;
using WifiActivationOrchestration.Api.Models.External;
using WifiActivationOrchestration.Api.Models.Requests;
using WifiActivationOrchestration.Api.Models.Responses;
using WifiActivationOrchestration.Api.Models.Results;
using WifiActivationOrchestration.Api.Services.External;

namespace WifiActivationOrchestration.Api.Services;

public sealed class WifiActivationService : IWifiActivationService
{
    private readonly INetworkInfrastructureClient _infrastructureClient;
    private readonly INetworkControllerClient _controllerClient;
    private readonly ILogger<WifiActivationService> _logger;

    public WifiActivationService(
        INetworkInfrastructureClient infrastructureClient,
        INetworkControllerClient controllerClient,
        ILogger<WifiActivationService> logger)
    {
        _infrastructureClient = infrastructureClient;
        _controllerClient = controllerClient;
        _logger = logger;
    }

    public async Task<WifiActivationResult> ActivateAsync(
        CustomerActivationRequest request,
        CancellationToken cancellationToken)
    {
        var command = CustomerActivationRequestMapper.ToCommand(request);

        if (command is null)
        {
            return WifiActivationResult.InvalidRequest(
                "Request is missing customerId, customerAddress, or speedProfile.");
        }

        try
        {
            var infrastructureResponse =
                await _infrastructureClient.GetSpeedProfilesAsync(cancellationToken);

            var speedProfile = infrastructureResponse.SpeedProfiles.FirstOrDefault(profile =>
                string.Equals(
                    profile.Code,
                    command.SpeedProfile,
                    StringComparison.OrdinalIgnoreCase));

            if (speedProfile is null)
            {
                return WifiActivationResult.SpeedProfileNotFound(
                    $"Speed profile '{command.SpeedProfile}' was not found.");
            }

            var controllerRequest = new NetworkControllerActivationRequest
            {
                CustomerId = command.CustomerId,
                CustomerAddress = command.CustomerAddress,
                UpstreamSpeed = speedProfile.UploadSpeedMbps,
                DownstreamSpeed = speedProfile.DownloadSpeedMbps
            };

            await _controllerClient.ActivateWifiAsync(
                controllerRequest,
                cancellationToken);

            var response = new WifiActivationResponse
            {
                ExternalId = command.ExternalId,
                CustomerId = command.CustomerId,
                CustomerAddress = command.CustomerAddress,
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
                command.CustomerId);

            return WifiActivationResult.DependencyFailure(
                "An external network API returned an invalid response.");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "WiFi activation timed out for customer {CustomerId}.",
                command.CustomerId);

            return WifiActivationResult.DependencyTimeout(
                "A timeout occurred while communicating with an external network API.");
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(
                exception,
                "WiFi activation failed while communicating with an external network API for customer {CustomerId}.",
                command.CustomerId);

            return WifiActivationResult.DependencyFailure(
                "Failed to communicate with an external network API.");
        }
    }
}
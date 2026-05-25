using WifiActivationOrchestration.Api.Clients;
using WifiActivationOrchestration.Api.Models;

namespace WifiActivationOrchestration.Api.Services;

public sealed class WifiActivationService : IWifiActivationService
{
    private const string CustomerIdCharacteristic = "customerId";
    private const string CustomerAddressCharacteristic = "customerAddress";
    private const string SpeedProfileCharacteristic = "speedProfile";

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
        var customerId = GetCharacteristicValue(request, CustomerIdCharacteristic);
        var customerAddress = GetCharacteristicValue(request, CustomerAddressCharacteristic);
        var requestedSpeedProfile = GetCharacteristicValue(request, SpeedProfileCharacteristic);

        if (string.IsNullOrWhiteSpace(customerId) ||
            string.IsNullOrWhiteSpace(customerAddress) ||
            string.IsNullOrWhiteSpace(requestedSpeedProfile))
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
                    requestedSpeedProfile,
                    StringComparison.OrdinalIgnoreCase));

            if (speedProfile is null)
            {
                return WifiActivationResult.SpeedProfileNotFound(
                    $"Speed profile '{requestedSpeedProfile}' was not found.");
            }

            var controllerRequest = new NetworkControllerActivationRequest
            {
                CustomerId = customerId,
                CustomerAddress = customerAddress,
                UpstreamSpeed = speedProfile.UploadSpeedMbps,
                DownstreamSpeed = speedProfile.DownloadSpeedMbps
            };

            await _controllerClient.ActivateWifiAsync(
                controllerRequest,
                cancellationToken);

            var response = new WifiActivationResponse
            {
                ExternalId = request.ExternalId,
                CustomerId = customerId,
                CustomerAddress = customerAddress,
                SpeedProfile = speedProfile.Code,
                UpstreamSpeed = speedProfile.UploadSpeedMbps,
                DownstreamSpeed = speedProfile.DownloadSpeedMbps,
                Status = "accepted"
            };

            return WifiActivationResult.Accepted(response);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "WiFi activation timed out for customer {CustomerId}.",
                customerId);

            return WifiActivationResult.DependencyTimeout(
                "A timeout occurred while communicating with an external network API.");
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(
                exception,
                "WiFi activation failed while communicating with an external network API for customer {CustomerId}.",
                customerId);

            return WifiActivationResult.DependencyFailure(
                "Failed to communicate with an external network API.");
        }
    }

    private static string? GetCharacteristicValue(
        CustomerActivationRequest request,
        string characteristicName)
    {
        var characteristic = request
            .OrderItem?
            .Service?
            .ServiceCharacteristic
            .FirstOrDefault(item =>
                string.Equals(
                    item.Name,
                    characteristicName,
                    StringComparison.OrdinalIgnoreCase));

        if (characteristic?.Value is null)
        {
            return null;
        }

        return characteristicName switch
        {
            CustomerIdCharacteristic => characteristic.Value.CustomerId,
            CustomerAddressCharacteristic => characteristic.Value.CustomerAddress,
            SpeedProfileCharacteristic => characteristic.Value.SpeedProfile,
            _ => null
        };
    }
}
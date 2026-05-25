using WifiActivationOrchestration.Api.Models;

namespace WifiActivationOrchestration.Api.Services;

public sealed class WifiActivationService : IWifiActivationService
{
    private const string CustomerIdCharacteristic = "customerId";
    private const string CustomerAddressCharacteristic = "customerAddress";
    private const string SpeedProfileCharacteristic = "speedProfile";

    private static readonly Dictionary<string, (int DownloadSpeed, int UploadSpeed)> SpeedProfiles =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["SP-100"] = (DownloadSpeed: 100, UploadSpeed: 20),
            ["SP-300"] = (DownloadSpeed: 300, UploadSpeed: 50),
            ["SP-500"] = (DownloadSpeed: 500, UploadSpeed: 100),
            ["SP-1000"] = (DownloadSpeed: 1000, UploadSpeed: 200)
        };

    public Task<WifiActivationResponse> ActivateAsync(CustomerActivationRequest request, CancellationToken cancellationToken)
    {
        var customerId = GetCharacteristicValue(request, CustomerIdCharacteristic);
        var customerAddress = GetCharacteristicValue(request, CustomerAddressCharacteristic);
        var speedProfile = GetCharacteristicValue(request, SpeedProfileCharacteristic);

        if (string.IsNullOrWhiteSpace(customerId) ||
            string.IsNullOrWhiteSpace(customerAddress) ||
            string.IsNullOrWhiteSpace(speedProfile))
        {
            throw new InvalidOperationException(
                "Request is missing customerId, customerAddress, or speedProfile.");
        }

        if (!SpeedProfiles.TryGetValue(speedProfile, out var speeds))
        {
            throw new InvalidOperationException(
                $"Speed profile '{speedProfile}' was not found.");
        }

        var response = new WifiActivationResponse
        {
            ExternalId = request.ExternalId,
            CustomerId = customerId,
            CustomerAddress = customerAddress,
            SpeedProfile = speedProfile,
            UpstreamSpeed = speeds.UploadSpeed,
            DownstreamSpeed = speeds.DownloadSpeed,
            Status = "accepted"
        };

        return Task.FromResult(response);
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
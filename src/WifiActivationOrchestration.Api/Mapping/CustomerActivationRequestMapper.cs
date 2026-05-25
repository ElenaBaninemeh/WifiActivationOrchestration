using WifiActivationOrchestration.Api.Models.Requests;

namespace WifiActivationOrchestration.Api.Mapping;

public sealed record ActivationCommand(
    string? ExternalId,
    string CustomerId,
    string CustomerAddress,
    string SpeedProfile);

public static class CustomerActivationRequestMapper
{
    public static ActivationCommand? ToCommand(CustomerActivationRequest request)
    {
        var customerId = GetCharacteristicValue(request, CharacteristicNames.CustomerId);
        var customerAddress = GetCharacteristicValue(request, CharacteristicNames.CustomerAddress);
        var speedProfile = GetCharacteristicValue(request, CharacteristicNames.SpeedProfile);

        if (string.IsNullOrWhiteSpace(customerId) ||
            string.IsNullOrWhiteSpace(customerAddress) ||
            string.IsNullOrWhiteSpace(speedProfile))
        {
            return null;
        }

        return new ActivationCommand(
            request.ExternalId,
            customerId,
            customerAddress,
            speedProfile);
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
            CharacteristicNames.CustomerId => characteristic.Value.CustomerId,
            CharacteristicNames.CustomerName => characteristic.Value.CustomerName,
            CharacteristicNames.CustomerAddress => characteristic.Value.CustomerAddress,
            CharacteristicNames.SpeedProfile => characteristic.Value.SpeedProfile,
            _ => null
        };
    }
}
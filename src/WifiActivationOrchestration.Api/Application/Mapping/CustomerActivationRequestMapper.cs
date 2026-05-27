using WifiActivationOrchestration.Api.Contracts.Requests;
using WifiActivationOrchestration.Api.Domain.Constants;

namespace WifiActivationOrchestration.Api.Application.Mapping;

/// <summary>
/// Internal command used by the application service after extracting the required
/// WiFi activation data from the customer portal request.
/// </summary>
public sealed record WifiActivationInput(
    string? ExternalId,
    string CustomerId,
    string CustomerAddress,
    string SpeedProfile);

/// <summary>
/// Maps the customer portal activation request into the internal command used by
/// the WiFi activation use case.
/// </summary>
public static class CustomerActivationRequestMapper
{
    /// <summary>
    /// Extracts the required activation values from the incoming customer request.
    /// </summary>
    /// <param name="request">Customer activation request received by the API.</param>
    /// <returns>
    /// An <see cref="WifiActivationInput"/> when all required values are present;
    /// otherwise, <c>null</c>.
    /// </returns>
    public static WifiActivationInput? MapToActivationInput(CustomerActivationRequest request)
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

        return new WifiActivationInput(request.ExternalId, customerId, customerAddress, speedProfile);
    }

    /// <summary>
    /// Finds a service characteristic by name and returns the matching typed value.
    /// </summary>
    /// <remarks>
    /// Each characteristic stores its value in a different property of the same
    /// value object, for example <c>CustomerId</c>, <c>CustomerAddress</c>, or
    /// <c>SpeedProfile</c>. This method hides that request-specific structure from
    /// the application service.
    /// </remarks>
    private static string? GetCharacteristicValue(CustomerActivationRequest request, string characteristicName)
    {
        var characteristic = request.OrderItem?
            .Service?
            .ServiceCharacteristic
            .FirstOrDefault(item =>
                string.Equals(item.Name, characteristicName, StringComparison.OrdinalIgnoreCase));

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
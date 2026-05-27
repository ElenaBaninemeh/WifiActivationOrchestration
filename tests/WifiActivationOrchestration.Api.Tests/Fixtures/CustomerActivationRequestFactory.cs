using WifiActivationOrchestration.Api.Contracts.Requests;
using WifiActivationOrchestration.Api.Domain.Constants;

namespace WifiActivationOrchestration.Api.Tests.Fixtures;

/// <summary>
/// Creates reusable customer activation requests for unit and integration tests.
/// </summary>

public static class CustomerActivationRequestFactory
{
    public const string DefaultExternalId = "ACT-20251017-001";
    public const string DefaultCustomerId = "CUST-4589";
    public const string DefaultCustomerName = "Alice Johnson";
    public const string DefaultCustomerAddress = "Keizersgracht 123, 1015 CJ Amsterdam, Netherlands";
    public const string DefaultSpeedProfile = "SP-500";

    /// <summary>
    /// Creates a valid customer activation request using the default test values.
    /// </summary>
    /// <param name="speedProfile">
    /// Speed profile code to include in the request.
    /// </param>
    /// <returns>
    /// A valid customer activation request.
    /// </returns>
    public static CustomerActivationRequest CreateValid(string speedProfile = DefaultSpeedProfile)
    {
        return new CustomerActivationRequest
        {
            ExternalId = DefaultExternalId,
            Description = "Activate WiFi service for VFZ customer",
            OrderItem = new OrderItem
            {
                Id = "1",
                Service = new Service
                {
                    Id = "",
                    ServiceSpecification = new ServiceSpecification
                    {
                        Id = "SPEC-WIFI-001",
                        Name = "WiFi Service"
                    },
                    ServiceCharacteristic =
                    [
                        CreateCustomerIdCharacteristic(),
                        CreateCustomerNameCharacteristic(),
                        CreateCustomerAddressCharacteristic(),
                        CreateSpeedProfileCharacteristic(speedProfile)
                    ]
                }
            }
        };
    }

    /// <summary>
    /// Creates an invalid request by removing the required speed profile characteristic.
    /// </summary>
    /// <returns>
    /// A customer activation request missing the speed profile value.
    /// </returns>
    public static CustomerActivationRequest CreateWithoutSpeedProfile()
    {
        var request = CreateValid();

        request.OrderItem!.Service!.ServiceCharacteristic =
        [
            .. request.OrderItem.Service.ServiceCharacteristic
                .Where(characteristic =>
                    !string.Equals(
                        characteristic.Name,
                        CharacteristicNames.SpeedProfile,
                        StringComparison.OrdinalIgnoreCase))
        ];

        return request;
    }

    private static ServiceCharacteristic CreateCustomerIdCharacteristic()
    {
        return new ServiceCharacteristic
        {
            Name = CharacteristicNames.CustomerId,
            ValueType = "string",
            Value = new CharacteristicValue
            {
                Type = "string",
                CustomerId = DefaultCustomerId
            }
        };
    }

    private static ServiceCharacteristic CreateCustomerNameCharacteristic()
    {
        return new ServiceCharacteristic
        {
            Name = CharacteristicNames.CustomerName,
            ValueType = "string",
            Value = new CharacteristicValue
            {
                Type = "string",
                CustomerName = DefaultCustomerName
            }
        };
    }

    private static ServiceCharacteristic CreateCustomerAddressCharacteristic()
    {
        return new ServiceCharacteristic
        {
            Name = CharacteristicNames.CustomerAddress,
            ValueType = "string",
            Value = new CharacteristicValue
            {
                Type = "string",
                CustomerAddress = DefaultCustomerAddress
            }
        };
    }

    private static ServiceCharacteristic CreateSpeedProfileCharacteristic(
        string speedProfile)
    {
        return new ServiceCharacteristic
        {
            Name = CharacteristicNames.SpeedProfile,
            ValueType = "string",
            Value = new CharacteristicValue
            {
                Type = "string",
                SpeedProfile = speedProfile
            }
        };
    }
}
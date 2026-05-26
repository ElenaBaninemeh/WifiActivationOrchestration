using WifiActivationOrchestration.Api.Contracts.Requests;
using WifiActivationOrchestration.Api.Domain.Constants;

namespace WifiActivationOrchestration.Api.Tests.Fixtures;

public static class CustomerActivationRequestFactory
{
    public const string DefaultExternalId = "ACT-20251017-001";
    public const string DefaultCustomerId = "CUST-4589";
    public const string DefaultCustomerName = "Alice Johnson";
    public const string DefaultCustomerAddress = "Keizersgracht 123, 1015 CJ Amsterdam, Netherlands";
    public const string DefaultSpeedProfile = "SP-500";

    public static CustomerActivationRequest CreateValid(
        string speedProfile = DefaultSpeedProfile)
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
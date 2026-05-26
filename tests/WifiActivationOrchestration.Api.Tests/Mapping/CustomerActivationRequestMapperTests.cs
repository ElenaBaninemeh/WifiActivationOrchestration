using WifiActivationOrchestration.Api.Mapping;
using WifiActivationOrchestration.Api.Models.Requests;
using WifiActivationOrchestration.Api.Tests.Fixtures;

namespace WifiActivationOrchestration.Api.Tests.Mapping;

public sealed class CustomerActivationRequestMapperTests
{
    [Fact]
    public void ToCommand_WhenRequestHasRequiredCharacteristics_ReturnsActivationCommand()
    {
        var request = CustomerActivationRequestFactory.CreateValid();

        var command = CustomerActivationRequestMapper.ToCommand(request);

        Assert.NotNull(command);
        Assert.Equal(CustomerActivationRequestFactory.DefaultExternalId, command.ExternalId);
        Assert.Equal(CustomerActivationRequestFactory.DefaultCustomerId, command.CustomerId);
        Assert.Equal(CustomerActivationRequestFactory.DefaultCustomerAddress, command.CustomerAddress);
        Assert.Equal(CustomerActivationRequestFactory.DefaultSpeedProfile, command.SpeedProfile);
    }

    [Fact]
    public void ToCommand_WhenSpeedProfileIsMissing_ReturnsNull()
    {
        var request = CustomerActivationRequestFactory.CreateWithoutSpeedProfile();

        var command = CustomerActivationRequestMapper.ToCommand(request);

        Assert.Null(command);
    }

    [Fact]
    public void ToCommand_WhenCharacteristicNamesUseDifferentCasing_ReturnsActivationCommand()
    {
        var request = CustomerActivationRequestFactory.CreateValid();

        foreach (var characteristic in request.OrderItem!.Service!.ServiceCharacteristic)
        {
            characteristic.Name = characteristic.Name.ToUpperInvariant();
        }

        var command = CustomerActivationRequestMapper.ToCommand(request);

        Assert.NotNull(command);
        Assert.Equal(CustomerActivationRequestFactory.DefaultCustomerId, command.CustomerId);
        Assert.Equal(CustomerActivationRequestFactory.DefaultCustomerAddress, command.CustomerAddress);
        Assert.Equal(CustomerActivationRequestFactory.DefaultSpeedProfile, command.SpeedProfile);
    }

    [Fact]
    public void ToCommand_WhenOrderItemIsMissing_ReturnsNull()
    {
        var request = new CustomerActivationRequest
        {
            ExternalId = CustomerActivationRequestFactory.DefaultExternalId,
            Description = "Activate WiFi service for VFZ customer",
            OrderItem = null
        };

        var command = CustomerActivationRequestMapper.ToCommand(request);

        Assert.Null(command);
    }

    [Fact]
    public void ToCommand_WhenServiceIsMissing_ReturnsNull()
    {
        var request = new CustomerActivationRequest
        {
            ExternalId = CustomerActivationRequestFactory.DefaultExternalId,
            Description = "Activate WiFi service for VFZ customer",
            OrderItem = new OrderItem
            {
                Id = "1",
                Service = null
            }
        };

        var command = CustomerActivationRequestMapper.ToCommand(request);

        Assert.Null(command);
    }
}
using WifiActivationOrchestration.Api.Application.Mapping;
using WifiActivationOrchestration.Api.Contracts.Requests;
using WifiActivationOrchestration.Api.Tests.Fixtures;

namespace WifiActivationOrchestration.Api.Tests.Mapping;

/// <summary>
/// Tests for mapping the nested customer portal request into the internal
/// WiFi activation input used by the application service.
/// </summary>
public sealed class CustomerActivationRequestMapperTests
{
    /// <summary>
    /// Verifies that a valid request containing all required service characteristics
    /// is mapped to an internal activation input.
    /// </summary>
    [Fact]
    public void MapToActivationInput_WhenRequestHasRequiredCharacteristics_ReturnsActivationInput()
    {
        var request = CustomerActivationRequestFactory.CreateValid();

        var ActivationInput = CustomerActivationRequestMapper.MapToActivationInput(request);

        Assert.NotNull(ActivationInput);
        Assert.Equal(CustomerActivationRequestFactory.DefaultExternalId, ActivationInput.ExternalId);
        Assert.Equal(CustomerActivationRequestFactory.DefaultCustomerId, ActivationInput.CustomerId);
        Assert.Equal(CustomerActivationRequestFactory.DefaultCustomerAddress, ActivationInput.CustomerAddress);
        Assert.Equal(CustomerActivationRequestFactory.DefaultSpeedProfile, ActivationInput.SpeedProfile);
    }

    /// <summary>
    /// Verifies that the mapper returns null when the required speed profile
    /// characteristic is missing from the request.
    /// </summary>
    [Fact]
    public void MapToActivationInput_WhenSpeedProfileIsMissing_ReturnsNull()
    {
        var request = CustomerActivationRequestFactory.CreateWithoutSpeedProfile();

        var ActivationInput = CustomerActivationRequestMapper.MapToActivationInput(request);

        Assert.Null(ActivationInput);
    }

    /// <summary>
    /// Verifies that characteristic name matching is case-insensitive.
    /// </summary>
    [Fact]
    public void MapToActivationInput_WhenCharacteristicNamesUseDifferentCasing_ReturnsActivationInput()
    {
        var request = CustomerActivationRequestFactory.CreateValid();

        foreach (var characteristic in request.OrderItem!.Service!.ServiceCharacteristic)
        {
            characteristic.Name = characteristic.Name.ToUpperInvariant();
        }

        var ActivationInput = CustomerActivationRequestMapper.MapToActivationInput(request);

        Assert.NotNull(ActivationInput);
        Assert.Equal(CustomerActivationRequestFactory.DefaultCustomerId, ActivationInput.CustomerId);
        Assert.Equal(CustomerActivationRequestFactory.DefaultCustomerAddress, ActivationInput.CustomerAddress);
        Assert.Equal(CustomerActivationRequestFactory.DefaultSpeedProfile, ActivationInput.SpeedProfile);
    }

    /// <summary>
    /// Verifies that the mapper returns null when the order item is missing.
    /// </summary>
    [Fact]
    public void MapToActivationInput_WhenOrderItemIsMissing_ReturnsNull()
    {
        var request = new CustomerActivationRequest
        {
            ExternalId = CustomerActivationRequestFactory.DefaultExternalId,
            Description = "Activate WiFi service for VFZ customer",
            OrderItem = null
        };

        var ActivationInput = CustomerActivationRequestMapper.MapToActivationInput(request);

        Assert.Null(ActivationInput);
    }

    /// <summary>
    /// Verifies that the mapper returns null when the service object is missing.
    /// </summary>
    [Fact]
    public void MapToActivationInput_WhenServiceIsMissing_ReturnsNull()
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

        var ActivationInput = CustomerActivationRequestMapper.MapToActivationInput(request);

        Assert.Null(ActivationInput);
    }
}
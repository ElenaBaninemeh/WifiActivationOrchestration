using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using WifiActivationOrchestration.Api.Models.External;
using WifiActivationOrchestration.Api.Models.Results;
using WifiActivationOrchestration.Api.Services;
using WifiActivationOrchestration.Api.Services.External;
using WifiActivationOrchestration.Api.Tests.Fixtures;

namespace WifiActivationOrchestration.Api.Tests.Services;

public sealed class WifiActivationServiceTests
{
    private readonly INetworkInfrastructureClient _infrastructureClient;
    private readonly INetworkControllerClient _controllerClient;
    private readonly ILogger<WifiActivationService> _logger;

    public WifiActivationServiceTests()
    {
        _infrastructureClient = Substitute.For<INetworkInfrastructureClient>();
        _controllerClient = Substitute.For<INetworkControllerClient>();
        _logger = Substitute.For<ILogger<WifiActivationService>>();
    }

    [Fact]
    public async Task ActivateAsync_WhenRequestIsValid_SendsExpectedActivationRequest()
    {
        // Arrange
        var request = CustomerActivationRequestFactory.CreateValid();

        _infrastructureClient
            .GetSpeedProfilesAsync(Arg.Any<CancellationToken>())
            .Returns(NetworkInfrastructureResponseFactory.WithSpeedProfile());

        var service = CreateService();

        // Act
        var result = await service.ActivateAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(WifiActivationStatus.Accepted, result.Status);
        Assert.NotNull(result.Response);

        Assert.Equal(CustomerActivationRequestFactory.DefaultExternalId, result.Response.ExternalId);
        Assert.Equal(CustomerActivationRequestFactory.DefaultCustomerId, result.Response.CustomerId);
        Assert.Equal(CustomerActivationRequestFactory.DefaultSpeedProfile, result.Response.SpeedProfile);
        Assert.Equal(100, result.Response.UpstreamSpeed);
        Assert.Equal(500, result.Response.DownstreamSpeed);

        await _controllerClient
            .Received(1)
            .ActivateWifiAsync(
                Arg.Is<NetworkControllerActivationRequest>(controllerRequest =>
                    controllerRequest.CustomerId == CustomerActivationRequestFactory.DefaultCustomerId &&
                    controllerRequest.CustomerAddress == CustomerActivationRequestFactory.DefaultCustomerAddress &&
                    controllerRequest.UpstreamSpeed == 100 &&
                    controllerRequest.DownstreamSpeed == 500),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ActivateAsync_WhenRequiredCharacteristicIsMissing_ReturnsInvalidRequest()
    {
        // Arrange
        var request = CustomerActivationRequestFactory.CreateWithoutSpeedProfile();

        var service = CreateService();

        // Act
        var result = await service.ActivateAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(WifiActivationStatus.InvalidRequest, result.Status);
        Assert.Null(result.Response);
        Assert.Equal(
            "Request is missing customerId, customerAddress, or speedProfile.",
            result.ErrorMessage);

        await _infrastructureClient
            .DidNotReceive()
            .GetSpeedProfilesAsync(Arg.Any<CancellationToken>());

        await _controllerClient
            .DidNotReceive()
            .ActivateWifiAsync(
                Arg.Any<NetworkControllerActivationRequest>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ActivateAsync_WhenSpeedProfileDoesNotExist_ReturnsSpeedProfileNotFound()
    {
        // Arrange
        const string unknownSpeedProfile = "SP-999";

        var request = CustomerActivationRequestFactory.CreateValid(
            speedProfile: unknownSpeedProfile);

        _infrastructureClient
            .GetSpeedProfilesAsync(Arg.Any<CancellationToken>())
            .Returns(NetworkInfrastructureResponseFactory.WithSpeedProfile());

        var service = CreateService();

        // Act
        var result = await service.ActivateAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(WifiActivationStatus.SpeedProfileNotFound, result.Status);
        Assert.Null(result.Response);
        Assert.Equal(
            $"Speed profile '{unknownSpeedProfile}' was not found.",
            result.ErrorMessage);

        await _controllerClient
            .DidNotReceive()
            .ActivateWifiAsync(
                Arg.Any<NetworkControllerActivationRequest>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ActivateAsync_WhenInfrastructureApiFails_ReturnsDependencyFailure()
    {
        // Arrange
        var request = CustomerActivationRequestFactory.CreateValid();

        _infrastructureClient
            .GetSpeedProfilesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Infrastructure API failure"));

        var service = CreateService();

        // Act
        var result = await service.ActivateAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(WifiActivationStatus.DependencyFailure, result.Status);
        Assert.Null(result.Response);
        Assert.Equal(
            "Failed to communicate with an external network API.",
            result.ErrorMessage);

        await _controllerClient
            .DidNotReceive()
            .ActivateWifiAsync(
                Arg.Any<NetworkControllerActivationRequest>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ActivateAsync_WhenControllerApiFails_ReturnsDependencyFailure()
    {
        // Arrange
        var request = CustomerActivationRequestFactory.CreateValid();

        _infrastructureClient
            .GetSpeedProfilesAsync(Arg.Any<CancellationToken>())
            .Returns(NetworkInfrastructureResponseFactory.WithSpeedProfile());

        _controllerClient
            .ActivateWifiAsync(
                Arg.Any<NetworkControllerActivationRequest>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Controller API failure"));

        var service = CreateService();

        // Act
        var result = await service.ActivateAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(WifiActivationStatus.DependencyFailure, result.Status);
        Assert.Null(result.Response);
        Assert.Equal(
            "Failed to communicate with an external network API.",
            result.ErrorMessage);
    }

    private WifiActivationService CreateService()
    {
        return new WifiActivationService(
            _infrastructureClient,
            _controllerClient,
            _logger);
    }
}
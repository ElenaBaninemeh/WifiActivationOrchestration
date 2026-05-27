using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using WifiActivationOrchestration.Api.Application.Common;
using WifiActivationOrchestration.Api.Application.Services;
using WifiActivationOrchestration.Api.Infrastructure.Models;
using WifiActivationOrchestration.Api.Infrastructure.Services;
using WifiActivationOrchestration.Api.Tests.Fixtures;

namespace WifiActivationOrchestration.Api.Tests.Services;

/// <summary>
/// Tests for the WiFi activation application service.
/// </summary>
public sealed class WifiActivationServiceTests
{
    private readonly ISpeedProfileService _infrastructureClient;
    private readonly INetworkActivationService _controllerClient;
    private readonly ILogger<WifiActivationService> _logger;

    public WifiActivationServiceTests()
    {
        _infrastructureClient = Substitute.For<ISpeedProfileService>();
        _controllerClient = Substitute.For<INetworkActivationService>();
        _logger = Substitute.For<ILogger<WifiActivationService>>();
    }

    /// <summary>
    /// Verifies that a valid activation request resolves the selected speed profile
    /// and sends the expected payload to the network activation service.
    /// </summary>
    [Fact]
    public async Task ActivateAsync_WhenRequestIsValid_SendsExpectedActivationRequest()
    {
        // Arrange
        var request = CustomerActivationRequestFactory.CreateValid();

        _infrastructureClient
            .GetSpeedProfilesAsync(Arg.Any<CancellationToken>())
            .Returns(SpeedProfileResponseFactory.WithSpeedProfile());

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
                Arg.Is<NetworkActivationRequest>(controllerRequest =>
                    controllerRequest.CustomerId == CustomerActivationRequestFactory.DefaultCustomerId &&
                    controllerRequest.CustomerAddress == CustomerActivationRequestFactory.DefaultCustomerAddress &&
                    controllerRequest.UpstreamSpeed == 100 &&
                    controllerRequest.DownstreamSpeed == 500),
                Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that the service returns an invalid request result when required
    /// activation data is missing, without calling external network APIs.
    /// </summary>
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
                Arg.Any<NetworkActivationRequest>(),
                Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that the service returns a speed-profile-not-found result when
    /// the requested speed profile is not available from the infrastructure API.
    /// </summary>
    [Fact]
    public async Task ActivateAsync_WhenSpeedProfileDoesNotExist_ReturnsSpeedProfileNotFound()
    {
        // Arrange
        const string unknownSpeedProfile = "SP-999";

        var request = CustomerActivationRequestFactory.CreateValid(
            speedProfile: unknownSpeedProfile);

        _infrastructureClient
            .GetSpeedProfilesAsync(Arg.Any<CancellationToken>())
            .Returns(SpeedProfileResponseFactory.WithSpeedProfile());

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
                Arg.Any<NetworkActivationRequest>(),
                Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that an HTTP failure from the speed profile service is translated
    /// into a dependency failure result.
    /// </summary>
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
                Arg.Any<NetworkActivationRequest>(),
                Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that an HTTP failure from the network activation service is
    /// translated into a dependency failure result.
    /// </summary>
    [Fact]
    public async Task ActivateAsync_WhenControllerApiFails_ReturnsDependencyFailure()
    {
        // Arrange
        var request = CustomerActivationRequestFactory.CreateValid();

        _infrastructureClient
            .GetSpeedProfilesAsync(Arg.Any<CancellationToken>())
            .Returns(SpeedProfileResponseFactory.WithSpeedProfile());

        _controllerClient
            .ActivateWifiAsync(
                Arg.Any<NetworkActivationRequest>(),
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

    /// <summary>
    /// Verifies that a timeout while retrieving speed profiles is translated
    /// into a dependency timeout result.
    /// </summary>
    [Fact]
    public async Task ActivateAsync_WhenInfrastructureApiTimesOut_ReturnsDependencyTimeout()
    {
        var request = CustomerActivationRequestFactory.CreateValid();

        _infrastructureClient
            .GetSpeedProfilesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Infrastructure API timeout"));

        var service = CreateService();

        var result = await service.ActivateAsync(request, CancellationToken.None);

        Assert.Equal(WifiActivationStatus.DependencyTimeout, result.Status);
        Assert.Null(result.Response);
        Assert.Equal(
            "A timeout occurred while communicating with an external network API.",
            result.ErrorMessage);

        await _controllerClient
            .DidNotReceive()
            .ActivateWifiAsync(
                Arg.Any<NetworkActivationRequest>(),
                Arg.Any<CancellationToken>());
    }

    private WifiActivationService CreateService()
    {
        return new WifiActivationService(
            _infrastructureClient,
            _controllerClient,
            _logger);
    }
}
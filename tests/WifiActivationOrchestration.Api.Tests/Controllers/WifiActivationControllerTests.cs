using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WifiActivationOrchestration.Api.Controllers;
using WifiActivationOrchestration.Api.Models.Responses;
using WifiActivationOrchestration.Api.Models.Results;
using WifiActivationOrchestration.Api.Services;
using WifiActivationOrchestration.Api.Tests.Fixtures;

namespace WifiActivationOrchestration.Api.Tests.Controllers;

public sealed class WifiActivationControllerTests
{
    private readonly IWifiActivationService _wifiActivationService;
    private readonly WifiActivationController _controller;

    public WifiActivationControllerTests()
    {
        _wifiActivationService = Substitute.For<IWifiActivationService>();

        _controller = new WifiActivationController(_wifiActivationService);
    }

    [Fact]
    public async Task ActivateWifiAsync_WhenServiceReturnsAccepted_ReturnsAccepted()
    {
        var request = CustomerActivationRequestFactory.CreateValid();

        var serviceResponse = new WifiActivationResponse
        {
            ExternalId = CustomerActivationRequestFactory.DefaultExternalId,
            CustomerId = CustomerActivationRequestFactory.DefaultCustomerId,
            CustomerAddress = CustomerActivationRequestFactory.DefaultCustomerAddress,
            SpeedProfile = CustomerActivationRequestFactory.DefaultSpeedProfile,
            UpstreamSpeed = 100,
            DownstreamSpeed = 500,
            Status = "accepted"
        };

        _wifiActivationService
            .ActivateAsync(request, Arg.Any<CancellationToken>())
            .Returns(WifiActivationResult.Accepted(serviceResponse));

        var result = await _controller.ActivateWifiAsync(
            request,
            CancellationToken.None);

        var acceptedResult = Assert.IsType<AcceptedResult>(result.Result);
        var response = Assert.IsType<WifiActivationResponse>(acceptedResult.Value);

        Assert.Equal(CustomerActivationRequestFactory.DefaultCustomerId, response.CustomerId);
        Assert.Equal(CustomerActivationRequestFactory.DefaultSpeedProfile, response.SpeedProfile);
        Assert.Equal(100, response.UpstreamSpeed);
        Assert.Equal(500, response.DownstreamSpeed);
    }

    [Fact]
    public async Task ActivateWifiAsync_WhenServiceReturnsInvalidRequest_ReturnsBadRequest()
    {
        var request = CustomerActivationRequestFactory.CreateWithoutSpeedProfile();

        _wifiActivationService
            .ActivateAsync(request, Arg.Any<CancellationToken>())
            .Returns(WifiActivationResult.InvalidRequest(
                "Request is missing customerId, customerAddress, or speedProfile."));

        var result = await _controller.ActivateWifiAsync(
            request,
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task ActivateWifiAsync_WhenServiceReturnsSpeedProfileNotFound_ReturnsNotFound()
    {
        var request = CustomerActivationRequestFactory.CreateValid("SP-999");

        _wifiActivationService
            .ActivateAsync(request, Arg.Any<CancellationToken>())
            .Returns(WifiActivationResult.SpeedProfileNotFound(
                "Speed profile 'SP-999' was not found."));

        var result = await _controller.ActivateWifiAsync(
            request,
            CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task ActivateWifiAsync_WhenServiceReturnsDependencyFailure_ReturnsBadGateway()
    {
        var request = CustomerActivationRequestFactory.CreateValid();

        _wifiActivationService
            .ActivateAsync(request, Arg.Any<CancellationToken>())
            .Returns(WifiActivationResult.DependencyFailure(
                "Failed to communicate with an external network API."));

        var result = await _controller.ActivateWifiAsync(
            request,
            CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(502, objectResult.StatusCode);
    }

    [Fact]
    public async Task ActivateWifiAsync_WhenServiceReturnsDependencyTimeout_ReturnsGatewayTimeout()
    {
        var request = CustomerActivationRequestFactory.CreateValid();

        _wifiActivationService
            .ActivateAsync(request, Arg.Any<CancellationToken>())
            .Returns(WifiActivationResult.DependencyTimeout(
                "A timeout occurred while communicating with an external network API."));

        var result = await _controller.ActivateWifiAsync(
            request,
            CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(504, objectResult.StatusCode);
    }
}
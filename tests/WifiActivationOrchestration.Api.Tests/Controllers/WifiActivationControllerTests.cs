using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WifiActivationOrchestration.Api.Application.Common;
using WifiActivationOrchestration.Api.Application.Services;
using WifiActivationOrchestration.Api.Contracts.Responses;
using WifiActivationOrchestration.Api.Controllers;
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

    /// <summary>
    /// Verifies that the controller returns HTTP 202 Accepted when the activation
    /// service successfully accepts the WiFi activation request.
    /// </summary>
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

    /// <summary>
    /// Verifies that validation failures from the application service are mapped
    /// to HTTP 400 Bad Request with a structured ProblemDetails response.
    /// </summary>
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

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);

        Assert.Equal(400, problemDetails.Status);
        Assert.Equal("Invalid WiFi activation request", problemDetails.Title);
    }

    /// <summary>
    /// Verifies that an unknown requested speed profile is mapped to
    /// HTTP 404 Not Found with a structured ProblemDetails response.
    /// </summary>
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

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(notFoundResult.Value);

        Assert.Equal(404, problemDetails.Status);
        Assert.Equal("Speed profile not found", problemDetails.Title);
    }

    /// <summary>
    /// Verifies that failures while communicating with an external network API
    /// are mapped to HTTP 502 Bad Gateway.
    /// </summary>
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
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);

        Assert.Equal(502, objectResult.StatusCode);
        Assert.Equal(502, problemDetails.Status);
        Assert.Equal("External dependency failure", problemDetails.Title);
    }

    /// <summary>
    /// Verifies that external network API timeouts are mapped to
    /// HTTP 504 Gateway Timeout.
    /// </summary>
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
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);

        Assert.Equal(504, objectResult.StatusCode);
        Assert.Equal(504, problemDetails.Status);
        Assert.Equal("External dependency timeout", problemDetails.Title);
    }
}
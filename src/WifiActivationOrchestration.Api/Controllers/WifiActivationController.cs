using Microsoft.AspNetCore.Mvc;
using WifiActivationOrchestration.Api.Models;
using WifiActivationOrchestration.Api.Services;

namespace WifiActivationOrchestration.Api.Controllers;

[ApiController]
[Route("api/wifi-activations")]
public sealed class WifiActivationController : ControllerBase
{
    private readonly IWifiActivationService _activationService;

    public WifiActivationController(IWifiActivationService activationService)
    {
        _activationService = activationService;
    }

    [HttpPost]
    public async Task<ActionResult<WifiActivationResponse>> ActivateWifiAsync(
        [FromBody] CustomerActivationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _activationService.ActivateAsync(
            request,
            cancellationToken);

        return result.Status switch
        {
            WifiActivationStatus.Accepted => Accepted(
                $"/api/wifi-activations/{result.Response!.ExternalId}",
                result.Response),

            WifiActivationStatus.InvalidRequest => BadRequest(CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Invalid WiFi activation request",
                result.ErrorMessage)),

            WifiActivationStatus.SpeedProfileNotFound => NotFound(CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "Speed profile not found",
                result.ErrorMessage)),

            WifiActivationStatus.DependencyFailure => StatusCode(
                StatusCodes.Status502BadGateway,
                CreateProblemDetails(
                    StatusCodes.Status502BadGateway,
                    "External dependency failure",
                    result.ErrorMessage)),

            WifiActivationStatus.DependencyTimeout => StatusCode(
                StatusCodes.Status504GatewayTimeout,
                CreateProblemDetails(
                    StatusCodes.Status504GatewayTimeout,
                    "External dependency timeout",
                    result.ErrorMessage)),

            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                CreateProblemDetails(
                    StatusCodes.Status500InternalServerError,
                    "Unexpected activation failure",
                    "An unexpected error occurred while activating WiFi."))
        };
    }

    private static ProblemDetails CreateProblemDetails(
        int statusCode,
        string title,
        string? detail)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };
    }
}
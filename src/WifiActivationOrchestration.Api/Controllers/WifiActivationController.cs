using Microsoft.AspNetCore.Mvc;
using WifiActivationOrchestration.Api.Application.Common;
using WifiActivationOrchestration.Api.Application.Services;
using WifiActivationOrchestration.Api.Contracts.Requests;
using WifiActivationOrchestration.Api.Contracts.Responses;

namespace WifiActivationOrchestration.Api.Controllers;

/// <summary>
/// Exposes the WiFi activation endpoint used by the customer portal.
/// </summary>
/// <remarks>
/// The controller is responsible:
/// receiving the activation request, delegating the orchestration to the application service,
/// and mapping the application result to the appropriate HTTP response.
/// </remarks>
[ApiController]
[Route("api/wifi-activations")]
public sealed class WifiActivationController : ControllerBase
{
    private readonly IWifiActivationService _activationService;

    public WifiActivationController(IWifiActivationService activationService)
    {
        _activationService = activationService;
    }

    /// <summary>
    /// Starts the WiFi activation flow for a customer order.
    /// </summary>
    /// <param name="request">
    /// Customer activation order received from the customer portal.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request when the HTTP request is aborted.
    /// </param>
    /// <returns>
    /// A 202 Accepted response when activation is accepted, or a structured error response
    /// when validation or external dependency failures occur.
    /// </returns>
    [HttpPost]
    [ProducesResponseType(typeof(WifiActivationResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status504GatewayTimeout)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WifiActivationResponse>> ActivateWifiAsync([FromBody] CustomerActivationRequest? request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest, "Invalid WiFi activation request", "Request body is required."));
        }

        var result = await _activationService.ActivateAsync(request,cancellationToken);

        return MapToHttpResponse(result);
    }

    /// <summary>
    /// Maps the application-level activation result to the HTTP response returned by the API.
    /// </summary>
    private ActionResult<WifiActivationResponse> MapToHttpResponse(WifiActivationResult result)
    {
        return result.Status switch
        {
            WifiActivationStatus.Accepted when result.Response is not null =>
                Accepted($"/api/wifi-activations/{result.Response.ExternalId}", result.Response),

            WifiActivationStatus.InvalidRequest =>
                BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest, "Invalid WiFi activation request",result.ErrorMessage)),

            WifiActivationStatus.SpeedProfileNotFound =>
                NotFound(CreateProblemDetails(StatusCodes.Status404NotFound, "Speed profile not found", result.ErrorMessage)),

            WifiActivationStatus.DependencyFailure =>
                StatusCode(StatusCodes.Status502BadGateway,
                    CreateProblemDetails(StatusCodes.Status502BadGateway, "External dependency failure", result.ErrorMessage)),

            WifiActivationStatus.DependencyTimeout =>
                StatusCode(StatusCodes.Status504GatewayTimeout,
                    CreateProblemDetails(StatusCodes.Status504GatewayTimeout, "External dependency timeout", result.ErrorMessage)),

            _ => StatusCode(
                    StatusCodes.Status500InternalServerError,
                    CreateProblemDetails(StatusCodes.Status500InternalServerError, "Unexpected activation failure", "An unexpected error occurred while activating WiFi."))
        };
    }

    /// <summary>
    /// Creates a structured error response using the standard ASP.NET Core ProblemDetails format.
    /// </summary>
    private static ProblemDetails CreateProblemDetails( int statusCode, string title, string? detail)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };
    }
}
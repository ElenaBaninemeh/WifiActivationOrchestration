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
        var response = await _activationService.ActivateAsync(
            request,
            cancellationToken);

        return Accepted(
            $"/api/wifi-activations/{response.ExternalId}",
            response);
    }
}
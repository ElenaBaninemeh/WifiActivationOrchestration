using WifiActivationOrchestration.Api.Models.Responses;

namespace WifiActivationOrchestration.Api.Models.Results;

public enum WifiActivationStatus
{
    Accepted,
    InvalidRequest,
    SpeedProfileNotFound,
    DependencyFailure,
    DependencyTimeout
}

public sealed class WifiActivationResult
{
    public WifiActivationStatus Status { get; init; }

    public WifiActivationResponse? Response { get; init; }

    public string? ErrorMessage { get; init; }

    public static WifiActivationResult Accepted(WifiActivationResponse response)
        => new()
        {
            Status = WifiActivationStatus.Accepted,
            Response = response
        };

    public static WifiActivationResult InvalidRequest(string message)
        => new()
        {
            Status = WifiActivationStatus.InvalidRequest,
            ErrorMessage = message
        };

    public static WifiActivationResult SpeedProfileNotFound(string message)
        => new()
        {
            Status = WifiActivationStatus.SpeedProfileNotFound,
            ErrorMessage = message
        };

    public static WifiActivationResult DependencyFailure(string message)
        => new()
        {
            Status = WifiActivationStatus.DependencyFailure,
            ErrorMessage = message
        };

    public static WifiActivationResult DependencyTimeout(string message)
        => new()
        {
            Status = WifiActivationStatus.DependencyTimeout,
            ErrorMessage = message
        };
}
using WifiActivationOrchestration.Api.Contracts.Responses;

namespace WifiActivationOrchestration.Api.Application.Common;

/// <summary>
/// Represents the possible outcomes of the WiFi activation use case.
/// </summary>
public enum WifiActivationStatus
{
    /// <summary>
    /// The activation request was accepted by the external network activation API.
    /// </summary>
    Accepted,

    /// <summary>
    /// The incoming request is missing required activation data.
    /// </summary>
    InvalidRequest,

    /// <summary>
    /// The requested speed profile does not exist in the available speed profiles.
    /// </summary>
    SpeedProfileNotFound,

    /// <summary>
    /// An external network API failed or returned an invalid response.
    /// </summary>
    DependencyFailure,

    /// <summary>
    /// Communication with an external network API timed out.
    /// </summary>
    DependencyTimeout
}

/// <summary>
/// Application-level result returned by the WiFi activation use case.
/// </summary>
public sealed class WifiActivationResult
{
    /// <summary>
    /// Gets the activation outcome.
    /// </summary>
    public WifiActivationStatus Status { get; init; }

    /// <summary>
    /// Gets the successful activation response.
    /// </summary>
    /// <remarks>
    /// This value is only populated when <see cref="Status"/> is
    /// <see cref="WifiActivationStatus.Accepted"/>. For all failure statuses,
    /// this value is <c>null</c> and <see cref="ErrorMessage"/> contains the reason.
    /// </remarks>
    public WifiActivationResponse? Response { get; init; }

    /// <summary>
    /// Gets the error message for unsuccessful activation results.
    /// </summary>
    /// <remarks>
    /// This value is populated for statuses such as
    /// <see cref="WifiActivationStatus.InvalidRequest"/>,
    /// <see cref="WifiActivationStatus.SpeedProfileNotFound"/>,
    /// <see cref="WifiActivationStatus.DependencyFailure"/>, and
    /// <see cref="WifiActivationStatus.DependencyTimeout"/>.
    /// For <see cref="WifiActivationStatus.Accepted"/>, this value is usually <c>null</c>.
    /// </remarks>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Creates a successful activation result.
    /// </summary>
    public static WifiActivationResult Accepted(WifiActivationResponse response)
        => new()
        {
            Status = WifiActivationStatus.Accepted,
            Response = response
        };

    /// <summary>
    /// Creates a result for an invalid customer activation request.
    /// </summary>
    public static WifiActivationResult InvalidRequest(string message)
        => new()
        {
            Status = WifiActivationStatus.InvalidRequest,
            ErrorMessage = message
        };

    /// <summary>
    /// Creates a result for a requested speed profile that could not be found.
    /// </summary>
    public static WifiActivationResult SpeedProfileNotFound(string message)
        => new()
        {
            Status = WifiActivationStatus.SpeedProfileNotFound,
            ErrorMessage = message
        };

    /// <summary>
    /// Creates a result for an external dependency failure.
    /// </summary>
    public static WifiActivationResult DependencyFailure(string message)
        => new()
        {
            Status = WifiActivationStatus.DependencyFailure,
            ErrorMessage = message
        };

    /// <summary>
    /// Creates a result for an external dependency timeout.
    /// </summary>
    public static WifiActivationResult DependencyTimeout(string message)
        => new()
        {
            Status = WifiActivationStatus.DependencyTimeout,
            ErrorMessage = message
        };
}
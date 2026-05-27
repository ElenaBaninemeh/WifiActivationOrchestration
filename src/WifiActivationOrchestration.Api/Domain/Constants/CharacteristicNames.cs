namespace WifiActivationOrchestration.Api.Domain.Constants;

/// <summary>
/// Defines the service characteristic names expected in the customer activation request.
/// </summary>
/// <remarks>
/// These values are used by the request mapper to extract the required activation data
/// from the service-characteristic structure received from the customer portal.
/// </remarks>
public static class CharacteristicNames
{
    /// <summary>
    /// Characteristic name containing the customer identifier.
    /// </summary>
    public const string CustomerId = "customerId";

    /// <summary>
    /// Characteristic name containing the customer name.
    /// </summary>
    public const string CustomerName = "customerName";

    /// <summary>
    /// Characteristic name containing the customer installation address.
    /// </summary>
    public const string CustomerAddress = "customerAddress";

    /// <summary>
    /// Characteristic name containing the requested speed profile code.
    /// </summary>
    public const string SpeedProfile = "speedProfile";
}
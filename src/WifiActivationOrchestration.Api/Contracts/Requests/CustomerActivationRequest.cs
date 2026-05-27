namespace WifiActivationOrchestration.Api.Contracts.Requests;

/// <summary>
/// Represents the customer portal request used to start a WiFi activation.
/// </summary>
/// <remarks>
/// This model reflects the incoming JSON contract. The application maps this
/// nested request structure to a simpler internal activation input before
/// running the orchestration flow.
/// </remarks>
public sealed class CustomerActivationRequest
{
    /// <summary>
    /// Gets or sets the external activation/order identifier.
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the request description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the order item containing the requested WiFi service details.
    /// </summary>
    public OrderItem? OrderItem { get; set; }
}

/// <summary>
/// Represents an order item from the customer activation request.
/// </summary>
public sealed class OrderItem
{
    /// <summary>
    /// Gets or sets the order item identifier.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the service requested by this order item.
    /// </summary>
    public Service? Service { get; set; }
}

/// <summary>
/// Represents the requested service and its characteristics.
/// </summary>
public sealed class Service
{
    /// <summary>
    /// Gets or sets the service identifier.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the service specification metadata.
    /// </summary>
    public ServiceSpecification? ServiceSpecification { get; set; }

    /// <summary>
    /// Gets or sets the characteristics containing customer and speed profile data.
    /// </summary>
    public List<ServiceCharacteristic> ServiceCharacteristic { get; set; } = [];
}

/// <summary>
/// Represents one named service characteristic from the customer request.
/// </summary>
public sealed class ServiceCharacteristic
{
    /// <summary>
    /// Gets or sets the characteristic name, such as customerId or speedProfile.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the value type from the incoming request.
    /// </summary>
    public string? ValueType { get; set; }

    /// <summary>
    /// Gets or sets the characteristic value object.
    /// </summary>
    public CharacteristicValue? Value { get; set; }
}

/// <summary>
/// Represents metadata about the requested service specification.
/// </summary>
public sealed class ServiceSpecification
{
    /// <summary>
    /// Gets or sets the service specification identifier.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the service specification name.
    /// </summary>
    public string? Name { get; set; }
}

/// <summary>
/// Represents the possible values carried by a service characteristic.
/// </summary>
/// <remarks>
/// The customer portal request stores different characteristic values in the
/// same object shape. The mapper reads the correct property based on the
/// characteristic name.
/// </remarks>
public sealed class CharacteristicValue
{
    /// <summary>
    /// Gets or sets the value type marker from the incoming request.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the customer identifier.
    /// </summary>
    public string? CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the customer name.
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Gets or sets the customer installation address.
    /// </summary>
    public string? CustomerAddress { get; set; }

    /// <summary>
    /// Gets or sets the requested speed profile code.
    /// </summary>
    public string? SpeedProfile { get; set; }
}

namespace WifiActivationOrchestration.Api.Contracts.Requests;

public sealed class CustomerActivationRequest
{
    public string? ExternalId { get; set; }

    public string? Description { get; set; }

    public OrderItem? OrderItem { get; set; }
}

public sealed class OrderItem
{
    public string? Id { get; set; }

    public Service? Service { get; set; }
}

public sealed class Service
{
    public string? Id { get; set; }

    public ServiceSpecification? ServiceSpecification { get; set; }

    public List<ServiceCharacteristic> ServiceCharacteristic { get; set; } = [];
}

public sealed class ServiceCharacteristic
{
    public string? Name { get; set; }

    public string? ValueType { get; set; }

    public CharacteristicValue? Value { get; set; }
}

public sealed class ServiceSpecification
{
    public string? Id { get; set; }

    public string? Name { get; set; }
}

public sealed class CharacteristicValue
{
    public string? Type { get; set; }

    public string? CustomerId { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerAddress { get; set; }

    public string? SpeedProfile { get; set; }
}

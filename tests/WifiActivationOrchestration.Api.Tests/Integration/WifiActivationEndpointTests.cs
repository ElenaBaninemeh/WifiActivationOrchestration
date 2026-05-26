using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WifiActivationOrchestration.Api.Configuration;
using WifiActivationOrchestration.Api.Models.Results;
using WifiActivationOrchestration.Api.Services;
using WifiActivationOrchestration.Api.Services.External;
using WifiActivationOrchestration.Api.Tests.Fixtures;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace WifiActivationOrchestration.Api.Tests.Integration;

public sealed class WifiActivationHttpIntegrationTests : IDisposable
{
    private const string SpeedProfilesPath = "/network-infrastructure/speed-profiles";
    private const string ActivationPath = "/network-controller/wifi/activations";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly WireMockServer _networkApiMock;

    public WifiActivationHttpIntegrationTests()
    {
        _networkApiMock = WireMockServer.Start();
    }

    [Fact]
    public async Task ActivateAsync_WhenRequestIsValid_SendsExpectedHttpPayloadToNetworkController()
    {
        // Arrange
        SetupSpeedProfilesMock();
        SetupSuccessfulControllerMock();

        var service = CreateService();
        var request = CustomerActivationRequestFactory.CreateValid();

        // Act
        var result = await service.ActivateAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(WifiActivationStatus.Accepted, result.Status);

        var controllerRequests = _networkApiMock.FindLogEntries(
            Request.Create()
                .WithPath(ActivationPath)
                .UsingPost());

        var controllerRequest = Assert.Single(controllerRequests);

        if (controllerRequest.RequestMessage?.Body is not { } requestBody)
        {
            throw new InvalidOperationException(
                "WireMock did not capture the outgoing Network Controller request body.");
        }

        using var payload = JsonDocument.Parse(requestBody);
        var root = payload.RootElement;

        Assert.Equal(
            CustomerActivationRequestFactory.DefaultCustomerId,
            root.GetProperty("customerId").GetString());

        Assert.Equal(
            CustomerActivationRequestFactory.DefaultCustomerAddress,
            root.GetProperty("customerAddress").GetString());

        Assert.Equal(100, root.GetProperty("upstreamSpeed").GetInt32());
        Assert.Equal(500, root.GetProperty("downstreamSpeed").GetInt32());

        Assert.False(root.TryGetProperty("externalId", out _));
        Assert.False(root.TryGetProperty("speedProfile", out _));
        Assert.False(root.TryGetProperty("status", out _));
    }

    private WifiActivationService CreateService()
    {
        var options = Options.Create(new NetworkApiOptions
        {
            InfrastructureBaseUrl = _networkApiMock.Url!,
            ControllerBaseUrl = _networkApiMock.Url!,
            SpeedProfilesPath = SpeedProfilesPath,
            ActivationPath = ActivationPath,
            TimeoutSeconds = 10
        });

        var infrastructureHttpClient = new HttpClient
        {
            BaseAddress = new Uri(_networkApiMock.Url!)
        };

        var controllerHttpClient = new HttpClient
        {
            BaseAddress = new Uri(_networkApiMock.Url!)
        };

        var infrastructureClient = new NetworkInfrastructureClient(
            infrastructureHttpClient,
            options,
            NullLogger<NetworkInfrastructureClient>.Instance);

        var controllerClient = new NetworkControllerClient(
            controllerHttpClient,
            options,
            NullLogger<NetworkControllerClient>.Instance);

        return new WifiActivationService(
            infrastructureClient,
            controllerClient,
            NullLogger<WifiActivationService>.Instance);
    }

    private void SetupSpeedProfilesMock()
    {
        var responseBody = JsonSerializer.Serialize(
            NetworkInfrastructureResponseFactory.WithSpeedProfile(),
            JsonOptions);

        _networkApiMock
            .Given(
                Request.Create()
                    .WithPath(SpeedProfilesPath)
                    .UsingGet())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(responseBody));
    }

    private void SetupSuccessfulControllerMock()
    {
        _networkApiMock
            .Given(
                Request.Create()
                    .WithPath(ActivationPath)
                    .UsingPost())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(202)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody("""
                    {
                      "status": "accepted"
                    }
                    """));
    }

    public void Dispose()
    {
        _networkApiMock.Stop();
        _networkApiMock.Dispose();
    }
}
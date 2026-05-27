using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

var server = WireMockServer.Start(new WireMockServerSettings
{
    Urls = ["http://0.0.0.0:8080"]
});

server
    .Given(
        Request.Create()
            .WithPath("/network-infrastructure/speed-profiles")
            .UsingGet())
    .RespondWith(
        Response.Create()
            .WithStatusCode(200)
            .WithHeader("Content-Type", "application/json")
            .WithBody("""
            {
              "requestId": "REQ-001",
              "speedProfiles": [
                {
                  "code": "SP-100",
                  "downloadSpeedMbps": 100,
                  "uploadSpeedMbps": 20
                },
                {
                  "code": "SP-300",
                  "downloadSpeedMbps": 300,
                  "uploadSpeedMbps": 50
                },
                {
                  "code": "SP-500",
                  "downloadSpeedMbps": 500,
                  "uploadSpeedMbps": 100
                },
                {
                  "code": "SP-1000",
                  "downloadSpeedMbps": 1000,
                  "uploadSpeedMbps": 200
                }
              ]
            }
            """));

server
    .Given(
        Request.Create()
            .WithPath("/network-controller/wifi/activations")
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

Console.WriteLine("WireMock server is running on http://0.0.0.0:8080");
Console.WriteLine("Mocked endpoints:");
Console.WriteLine("GET /network-infrastructure/speed-profiles");
Console.WriteLine("POST /network-controller/wifi/activations");

await Task.Delay(Timeout.Infinite);
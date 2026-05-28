## WiFi Activation Orchestration API

A REST API built with ASP.NET Core and .NET 10 for orchestrating WiFi activation.

The API receives a customer activation request, resolves the requested speed profile through a mocked Network Infrastructure API, and sends the final activation payload to a mocked Network Controller API.

## Quickstart

Start the API and mock server with Docker Compose:

```powershell
docker compose up --build
```

This starts:

| Component | URL |
|---|---|
| WiFi Activation API | `http://localhost:5184` |
| Mock network APIs | `http://localhost:8080` |

Send a request to:

```http
POST http://localhost:5184/api/wifi-activations
```

Run tests through Docker:

```powershell
docker compose build tests
docker compose run --rm tests
```

The API acts as an orchestration boundary, coordinating the request flow, transforming the incoming request into the required downstream payload, and returning a meaningful HTTP response to the caller.

### Flow steps

| Step | Responsibility |
|---|---|
| Receive request | The controller receives the customer activation request. |
| Extract activation data | The application service extracts `customerId`, `customerAddress`, and `speedProfile` from the nested request structure. |
| Resolve speed profile | The infrastructure service retrieves available speed profiles and the application service matches the requested profile. |
| Build activation payload | The application service combines customer data with resolved upstream/downstream speeds. |
| Submit activation | The network activation service sends the final payload to the Network Controller API. |
| Return result | The controller maps the application result to the appropriate HTTP response. |

---

## Project structure

```text
.
├── src/
│   └── WifiActivationOrchestration.Api/
│       ├── Controllers/                 HTTP entry points and response mapping
│       ├── Contracts/                   Request and response contracts exposed by the API
│       ├── Domain/                      Shared constants used by the activation flow
│       ├── Application/                 Use case orchestration, interfaces, mapping, result model
│       └── Infrastructure/              External HTTP integrations and configuration
│
├── tests/
│   └── WifiActivationOrchestration.Api.Tests/
│       ├── Controllers/                 Controller response mapping tests
│       ├── Fixtures/                    Reusable test data builders
│       ├── Integration/                 WireMock-based HTTP integration test
│       ├── Mapping/                     Request mapping tests
│       └── Services/                    Application service orchestration tests
│
├── tools/
│   └── WifiActivationOrchestration.MockServer/
│       └── WireMock-based local mock server
│
├── Dockerfile                           API container image
├── Dockerfile.tests                     Dedicated test container image
├── docker-compose.yml                   API, mock server, and test runner
├── coverlet.runsettings                 Coverage collection settings      
└── README.md
```

---

## Architecture
Clean Architecture is based on keeping the core application behavior independent from delivery mechanisms and external infrastructure.

| Clean Architecture concept | Implementation in this solution | Responsibility |
|---|---|---|
| Presentation / API layer | `Controllers` | Receives HTTP requests and maps application results to HTTP responses. |
| Contract layer | `Contracts` | Defines the JSON request and response models exposed by this API. |
| Application layer | `Application` | Coordinates the WiFi activation use case and defines the required downstream operations. |
| Domain layer | `Domain` | Holds shared domain constants used by the activation flow. |
| Infrastructure layer | `Infrastructure` | Implements communication with downstream network APIs and external configuration. |

### Presentation / API layer

`WifiActivationController` represents the outer API boundary. It accepts the incoming activation request, delegates the use case to the application layer, and converts the application result into the correct HTTP response.

### Contract layer

The request and response contracts are kept separate from the application and infrastructure models.

`CustomerActivationRequest` follows the incoming customer portal JSON shape, including the nested service-characteristic structure.

`WifiActivationResponse` represents the response returned by this API after an activation request is accepted.

The outbound Network Controller payload is represented separately as `NetworkActivationRequest`, because it belongs to an external downstream API contract rather than this API's public response contract.

### Application layer

The application layer contains the activation use case.

`WifiActivationService` coordinates the flow:

1. extract the required activation values from the incoming request,
2. retrieve available speed profiles,
3. resolve the requested speed profile,
4. create the outbound activation payload,
5. submit the activation request to the downstream network controller,
6. return an application-level result.
The application layer expresses expected outcomes through `WifiActivationResult` and `WifiActivationStatus`. The controller translates those outcomes to HTTP responses at the API boundary.

### Infrastructure layer

The infrastructure layer contains the concrete downstream API integrations.

`SpeedProfileService` retrieves available speed profiles from the configured Network Infrastructure API.

`NetworkActivationService` sends the final activation payload to the configured Network Controller API.

External endpoint settings are provided through `NetworkApiOptions`, keeping URLs, paths, and timeout values outside of the application use case.

---

## Endpoint

### `POST /api/wifi-activations`

Starts the WiFi activation flow for a customer order.

#### Example request

```json
{
  "externalId": "ACT-20251017-001",
  "description": "Activate WiFi service for VFZ customer",
  "orderItem": {
    "id": "1",
    "service": {
      "id": "",
      "serviceSpecification": {
        "id": "SPEC-WIFI-001",
        "name": "WiFi Service"
      },
      "serviceCharacteristic": [
        {
          "name": "customerId",
          "valueType": "string",
          "value": {
            "type": "string",
            "customerId": "CUST-4589"
          }
        },
        {
          "name": "customerName",
          "valueType": "string",
          "value": {
            "type": "string",
            "customerName": "Alice Johnson"
          }
        },
        {
          "name": "customerAddress",
          "valueType": "string",
          "value": {
            "type": "string",
            "customerAddress": "Keizersgracht 123, 1015 CJ Amsterdam, Netherlands"
          }
        },
        {
          "name": "speedProfile",
          "valueType": "string",
          "value": {
            "type": "string",
            "speedProfile": "SP-500"
          }
        }
      ]
    }
  }
}
```

#### Successful response

```http
202 Accepted
```

```json
{
  "externalId": "ACT-20251017-001",
  "customerId": "CUST-4589",
  "customerAddress": "Keizersgracht 123, 1015 CJ Amsterdam, Netherlands",
  "speedProfile": "SP-500",
  "upstreamSpeed": 100,
  "downstreamSpeed": 500,
  "status": "accepted"
}
```

The API returns `202 Accepted` after the downstream Network Controller API accepts the activation payload.

The response body summarizes the accepted activation request. The API does not return a `Location` header because it does not expose a separate activation-status endpoint.

## Example manual request

With Docker Compose running, send this request:

```powershell
$body = @'
{
  "externalId": "ACT-20251017-001",
  "description": "Activate WiFi service for VFZ customer",
  "orderItem": {
    "id": "1",
    "service": {
      "id": "",
      "serviceSpecification": {
        "id": "SPEC-WIFI-001",
        "name": "WiFi Service"
      },
      "serviceCharacteristic": [
        {
          "name": "customerId",
          "valueType": "string",
          "value": {
            "type": "string",
            "customerId": "CUST-4589"
          }
        },
        {
          "name": "customerAddress",
          "valueType": "string",
          "value": {
            "type": "string",
            "customerAddress": "Keizersgracht 123, 1015 CJ Amsterdam, Netherlands"
          }
        },
        {
          "name": "speedProfile",
          "valueType": "string",
          "value": {
            "type": "string",
            "speedProfile": "SP-500"
          }
        }
      ]
    }
  }
}
'@

Invoke-RestMethod `
  -Uri "http://localhost:5184/api/wifi-activations" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body
```

Expected result:

```http
202 Accepted
```

---

## Network Controller payload

The API response is intentionally different from the outbound payload sent to the Network Controller API.

The Network Controller receives only the data required for activation:

```json
{
  "customerId": "CUST-4589",
  "customerAddress": "Keizersgracht 123, 1015 CJ Amsterdam, Netherlands",
  "upstreamSpeed": 100,
  "downstreamSpeed": 500
}
```

This behavior is covered by the integration test, which verifies the actual HTTP request captured by WireMock.

---

## Error handling

Expected failures are translated into explicit HTTP responses.

| Scenario | HTTP status |
|---|---|
| Missing request body | `400 Bad Request` |
| Missing required activation data | `400 Bad Request` |
| Requested speed profile does not exist | `404 Not Found` |
| External API failure or invalid external response | `502 Bad Gateway` |
| External API timeout | `504 Gateway Timeout` |
| Unexpected failure | `500 Internal Server Error` |

Error responses use ASP.NET Core `ProblemDetails` for a consistent JSON error shape.

---
## Configuration

External network API settings are configured through the `NetworkApis` section.

```json
{
  "NetworkApis": {
    "InfrastructureBaseUrl": "http://localhost:8080",
    "ControllerBaseUrl": "http://localhost:8080",
    "SpeedProfilesPath": "/network-infrastructure/speed-profiles",
    "ActivationPath": "/network-controller/wifi/activations",
    "TimeoutSeconds": 10
  }
}
```

In Docker Compose, the API uses the mock server service name instead of `localhost`:

```text
http://mockserver:8080
```

This is required because each container has its own network namespace.

---

## Running locally without Docker

Start the mock server:

```powershell
dotnet run --project .\tools\WifiActivationOrchestration.MockServer\WifiActivationOrchestration.MockServer.csproj
```

Start the API in a second terminal:

```powershell
dotnet run --project .\src\WifiActivationOrchestration.Api\WifiActivationOrchestration.Api.csproj --urls "http://localhost:5184"
```

Then call:

```http
POST http://localhost:5184/api/wifi-activations
```

---

## Running with Docker

Start the API and mock server:

```powershell
docker compose up --build
```

Stop the containers:

```powershell
docker compose down
```

---

## Testing

Run all tests locally:

```powershell
dotnet test
```

Run tests through Docker:

```powershell
docker compose build tests
docker compose run --rm tests
```

### Test coverage

The test suite covers:

| Test area | Purpose |
|---|---|
| Controller tests | HTTP result mapping for accepted and failure outcomes. |
| Mapper tests | Extraction of required values from the nested customer request. |
| Service tests | Activation orchestration, validation behavior, dependency failures, and timeout handling. |
| Integration test | Verification of the outbound HTTP payload sent to the mocked Network Controller API. |

Collect coverage:

```powershell
dotnet test --settings coverlet.runsettings --collect:"XPlat Code Coverage"
```

Generate a readable coverage report:

```powershell
reportgenerator "-reports:tests\WifiActivationOrchestration.Api.Tests\TestResults\**\coverage.cobertura.xml" "-targetdir:coveragereport" "-reporttypes:Html;TextSummary"
```

Read the summary:

```powershell
Get-Content .\coveragereport\Summary.txt
```
Latest verified local result:

```text
Line coverage: 96.1%
```

Generated coverage output is excluded from Git.

---

## Manual verification scenarios

### Happy path

Use speed profile:

```json
"speedProfile": "SP-500"
```

Expected result:

```http
202 Accepted
```

### Unknown speed profile

Use speed profile:

```json
"speedProfile": "SP-999"
```

Expected result:

```http
404 Not Found
```

### Missing required values data

Remove one of the required values:

- `externalId`
- `customerId`
- `customerAddress`
- `speedProfile`

Expected result:

```http
400 Bad Request
```

### External dependency unavailable

Stop the mock server and call the API.

Expected result:

```http
502 Bad Gateway
```

---
## Verification checklist

```powershell
dotnet build
dotnet test
docker compose build
docker compose run --rm tests
```

For manual end-to-end verification:

```powershell
docker compose up --build
```

Then call:

```http
POST http://localhost:5184/api/wifi-activations
```

Expected result:

```http
202 Accepted
```

---

## Notes

- The mock server is included for local development and assessment verification.
- The downstream network APIs are represented by WireMock endpoints.
- Generated files such as `bin`, `obj`, `TestResults`, and `coveragereport` are excluded from Git.


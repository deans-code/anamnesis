## MODIFIED Requirements

### Requirement: Backend communicates with local Ollama instance using MedGemma
The .NET Core backend SHALL send chat requests to the Ollama API at `http://localhost:11434/api/chat` using the `POST` method with a JSON body containing the model name (`medgemma`) and conversation history. The browser SHALL NOT communicate with Ollama directly. No cloud-hosted LLM SHALL be used. All calls SHALL be protected by a circuit breaker and rate limiter as defined in the `circuit-breaker` capability. The Ollama adapter SHALL only call the `/api/chat` endpoint; no other Ollama API endpoints SHALL be called.

#### Scenario: Successful connection to Ollama
- **WHEN** the user initiates a conversation turn and the circuit breaker is closed
- **THEN** the backend SHALL send a well-formed request to the Ollama `/api/chat` endpoint, receive the response, and return the completed message to the Blazor UI

#### Scenario: Ollama is not running or unreachable
- **WHEN** the backend HTTP request to the Ollama endpoint fails (connection refused or timeout)
- **THEN** the backend SHALL log the error details and the UI SHALL display a simple error message indicating that the service is unavailable

#### Scenario: Circuit breaker is open
- **WHEN** the backend attempts to call Ollama and the circuit breaker is open
- **THEN** the backend SHALL immediately return an `OllamaUnavailableException` without making a network call, and the UI SHALL display the service unavailable message

### Requirement: Ollama endpoint and model are configured via application settings
The backend SHALL read both the Ollama base URL and model name from `appsettings.json` (`OllamaSettings:BaseUrl` and `OllamaSettings:Model`). The model name SHALL always be loaded from configuration and SHALL NOT be hardcoded. At application startup, `OllamaSettings` SHALL be validated: the configured model name MUST start with `medgemma` (case-insensitive); if it does not, the application SHALL fail to start with a descriptive `InvalidOperationException`. The base URL and model name SHALL NOT be changeable at runtime via the UI.

#### Scenario: Default configuration is used
- **WHEN** the application starts with the default `appsettings.json` containing `OllamaSettings:Model` set to `medgemma:27b`
- **THEN** the backend SHALL target `http://localhost:11434` and SHALL use `medgemma:27b` for all requests

#### Scenario: Operator overrides base URL or model variant
- **WHEN** `OllamaSettings:BaseUrl` or `OllamaSettings:Model` is overridden in `appsettings.json` or environment variables with a valid medgemma variant (e.g. `medgemma:7b`)
- **THEN** the backend SHALL use the overridden values for subsequent requests

#### Scenario: Operator sets a non-medgemma model name
- **WHEN** `OllamaSettings:Model` is set to a value that does not start with `medgemma`
- **THEN** the application SHALL fail to start with an `InvalidOperationException` describing the invalid model name

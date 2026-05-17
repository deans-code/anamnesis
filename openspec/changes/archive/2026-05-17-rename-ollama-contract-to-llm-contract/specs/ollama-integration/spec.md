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
- **THEN** the backend SHALL immediately return an `LlmUnavailableException` without making a network call, and the UI SHALL display the service unavailable message

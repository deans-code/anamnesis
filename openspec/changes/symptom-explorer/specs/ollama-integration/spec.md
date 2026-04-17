## ADDED Requirements

### Requirement: Backend communicates with local Ollama instance using MedGemma
The .NET Core backend SHALL send chat requests to the Ollama API at `http://localhost:11434/api/chat` using the `POST` method with a JSON body containing the model name (`medgemma`) and conversation history. The browser SHALL NOT communicate with Ollama directly. No cloud-hosted LLM SHALL be used.

#### Scenario: Successful connection to Ollama
- **WHEN** the user initiates a conversation turn
- **THEN** the backend SHALL send a well-formed request to the Ollama `/api/chat` endpoint, receive the response, and return the completed message to the Blazor UI

#### Scenario: Ollama is not running or unreachable
- **WHEN** the backend HTTP request to the Ollama endpoint fails (connection refused or timeout)
- **THEN** the backend SHALL log the error details and the UI SHALL display a simple error message indicating that the service is unavailable

### Requirement: Ollama endpoint and model are configured via application settings
The backend SHALL read the Ollama base URL and model name from `appsettings.json`. The model SHALL default to `medgemma`. The configuration SHALL not be changeable at runtime via the UI.

#### Scenario: Default configuration is used
- **WHEN** the application starts with no overridden settings
- **THEN** the backend SHALL target `http://localhost:11434` and use the model `medgemma` for all requests

#### Scenario: Operator overrides configuration
- **WHEN** the `OllamaSettings:BaseUrl` or `OllamaSettings:Model` values are overridden in `appsettings.json` or environment variables
- **THEN** the backend SHALL use the overridden values for subsequent Ollama requests

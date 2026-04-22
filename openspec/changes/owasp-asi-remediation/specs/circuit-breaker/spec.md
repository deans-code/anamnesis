## ADDED Requirements

### Requirement: All Ollama API calls are protected by a circuit breaker
The system SHALL wrap all HTTP calls to the Ollama API in a circuit breaker. The circuit breaker SHALL open after 5 consecutive failures and SHALL remain open for 30 seconds before allowing a probe request. While the circuit is open, the system SHALL immediately return an `OllamaUnavailableException` without attempting a network call.

#### Scenario: Circuit opens after repeated failures
- **WHEN** 5 consecutive Ollama HTTP requests fail (connection refused, timeout, or 5xx response)
- **THEN** the circuit breaker SHALL open and subsequent calls SHALL fail immediately with `OllamaUnavailableException` for 30 seconds

#### Scenario: Circuit closes after recovery
- **WHEN** the circuit is open and the 30-second window elapses
- **THEN** the circuit breaker SHALL allow a single probe request; if it succeeds, the circuit SHALL close and normal operation SHALL resume

#### Scenario: Circuit is closed under normal operation
- **WHEN** Ollama calls succeed consistently
- **THEN** the circuit breaker SHALL remain closed and calls SHALL proceed normally

### Requirement: LLM calls are rate-limited per session
The system SHALL enforce a maximum of 10 Ollama API calls per minute per session. Calls that exceed this limit SHALL be rejected with a `RateLimitExceededException` without reaching the Ollama API.

#### Scenario: Rate limit is not exceeded
- **WHEN** a session makes fewer than 10 LLM calls within a 60-second window
- **THEN** all calls SHALL proceed to the Ollama API

#### Scenario: Rate limit is exceeded
- **WHEN** a session makes more than 10 LLM calls within a 60-second window
- **THEN** the 11th and subsequent calls SHALL be rejected with `RateLimitExceededException` and SHALL NOT reach the Ollama API

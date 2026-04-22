## Why

The Anamnesis medical symptom explorer operates as an AI agent with no deterministic security controls — user input flows directly to an LLM without validation, responses are trusted for control flow without verification, and there is no audit trail or anomaly detection. An OWASP ASI compliance review (2026-04-22) found 8 of 10 controls failing, placing the application at HIGH risk.

## What Changes

- Add an input validation pipeline that intercepts user messages before they reach the LLM, performing content filtering and prompt injection detection
- Add a deterministic policy enforcement layer (allow/deny predicates) that runs outside the LLM and fails closed on error
- Add LLM response integrity verification before responses are used for control flow decisions
- Add structured audit logging for all LLM interactions, user inputs, session events, and policy decisions
- Add circuit breakers and rate limiting around all Ollama API calls
- Restrict the Ollama adapter to a validated allowlist of operations with argument constraints
- Define explicit agent capability boundaries (max message length, max session depth, allowed roles)

## Capabilities

### New Capabilities
- `input-validation`: Validates and sanitises user input before it reaches the LLM; detects prompt injection patterns; fails closed on policy error
- `policy-enforcement`: Deterministic allow/deny evaluation of user requests and LLM responses using code predicates; no LLM involvement; evaluates in <0.1ms
- `audit-logging`: Structured, append-only audit log of all LLM tool calls, user inputs, session lifecycle events, and policy decisions
- `circuit-breaker`: Circuit breaker and rate limiter wrapping all Ollama API calls; trust-score decay; kill-switch capability

### Modified Capabilities
- `llm-conversation`: Input validation and policy enforcement are now required steps in the `SendAsync` path; `CheckContinuationAsync` must verify response integrity before using output for control flow
- `ollama-integration`: The Ollama adapter is restricted to the `/api/chat` endpoint only; model name is loaded from `appsettings.json` and validated at startup to ensure it starts with `medgemma`; request parameters are bounded

## Impact

- **`Anamnesis.Interface.Website`**: Input validation added to the symptom input form component before submission reaches the use case
- **`Anamnesis.UseCase.Conversation`**: Policy enforcement, response integrity verification, and audit logging added within `ConversationService`
- **`Anamnesis.Adapter.Ollama`**: Circuit breaker, rate limiting, and adapter hardening (model loaded from config + startup validation, endpoint restriction, message count guard) added within `OllamaClient`
- **No new projects**: Security concerns are implemented in the layer they protect; no `Anamnesis.UseCase.Security` project is created
- **Dependencies**: `Microsoft.Extensions.Http.Resilience` added to `Anamnesis.Adapter.Ollama`; no other new external dependencies

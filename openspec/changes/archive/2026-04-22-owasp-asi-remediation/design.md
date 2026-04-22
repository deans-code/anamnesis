## Context

Anamnesis is a medical symptom explorer — a Blazor Server application that proxies user conversation through a local Ollama/MedGemma LLM. An OWASP ASI compliance review (2026-04-22) rated the application HIGH risk: 8 of 10 controls failed. The current architecture has a single code path — `ConversationService.SendAsync` — that takes a raw user string, appends it to history, and sends it to `OllamaClient.ChatAsync` with no validation, policy check, or audit step in between.

The change introduces four cross-cutting security layers that are composed into the existing conversation use case without replacing its core behaviour.

## Goals / Non-Goals

**Goals:**
- Close ASI-01 (prompt injection) via a deterministic input validation pipeline
- Close ASI-02 (insecure tool use) by restricting and validating the Ollama adapter
- Close ASI-03 (excessive agency) by defining explicit capability boundaries
- Close ASI-05 (trust boundary) by verifying LLM output before use in control flow
- Close ASI-06 (insufficient logging) via structured append-only audit logging
- Close ASI-08 (policy bypass) via a fail-closed deterministic policy evaluator
- Close ASI-10 (behavioral anomaly) via circuit breakers and rate limiting

**Non-Goals:**
- ASI-04 (escalation) — already passes; no changes needed
- ASI-07 (cryptographic identity) — deferred; requires DID/PKI infrastructure not yet in scope
- ASI-09 (supply chain) — deferred; requires build-pipeline SBOM tooling not yet in scope
- Adding authentication or multi-user support
- Replacing the MedGemma model or Ollama integration

## Decisions

### 1. Distribute security to the layer it protects — no new security project

**Decision**: Security is a cross-cutting concern implemented within the existing layer it protects:
- **Input validation** (user form data) → `Anamnesis.Interface.Website`: validates the symptom input before it is handed to the use case
- **Policy enforcement, response integrity, audit logging** (conversation business rules) → `Anamnesis.UseCase.Conversation`: added inside `ConversationService`
- **Circuit breaker, rate limiting, adapter hardening** (Ollama-specific) → `Anamnesis.Adapter.Ollama`: added inside `OllamaClient`

No `Anamnesis.UseCase.Security` project is created.

**Rationale**: The architecture rule is that each layer owns its responsibilities. Creating a security layer would violate separation of concerns by pulling behaviour out of the layers it belongs to. Input validation of a form field is an interface-layer concern; policy enforcement against conversation rules is a use-case concern; resilience of an external API call is an adapter concern.

**Alternatives considered**:
- *New `Anamnesis.UseCase.Security` project*: rejected — security is not a use case; extracting it creates an artificial layer and forces cross-layer dependencies

### 2. Fail-closed policy evaluator using code predicates

**Decision**: The `PolicyEvaluator` uses compiled code predicates (not YAML rules or LLM calls) to evaluate whether a request is allowed. On any evaluation error, the evaluator returns `Deny`.

**Rationale**: The OWASP finding is that the only policy today is the system prompt — entirely LLM-dependent. A deterministic evaluator must complete without any LLM call and must not fail open.

**Alternatives considered**:
- *YAML rule engine*: would require a runtime rule parser dependency; adds complexity without clear benefit for the current rule set
- *Second LLM call as guard*: explicitly rejected by ASI-08; adds latency and circular dependency

### 3. Input validation pipeline using pattern matching

**Decision**: Input validation runs a set of compiled regex patterns against user input to detect common prompt injection patterns (role-play directives, instruction overrides, system prompt references). Inputs exceeding `MaxMessageLength` (4 096 chars) are rejected immediately.

**Rationale**: Prompt injection detection using heuristics has known false-positive risks, but is the only deterministic option available without an external service. Conservative patterns (exact keyword phrases) reduce false positives.

**Alternatives considered**:
- *External prompt injection detection API*: adds a network dependency that may be unavailable; deferred to a future change
- *LLM-based intent classifier*: circular dependency and adds latency

### 4. LLM response integrity verification

**Decision**: `CheckContinuationAsync` wraps its regex with a strict allowlist check: only the literal strings `"END"` or `"CONTINUE"` (case-insensitive, trimmed) are treated as valid signals. Any other response is treated as `CONTINUE` (safe default).

**Rationale**: The current `Regex.IsMatch(response.Trim(), @"\bEND\b")` is fragile — any response containing the word "end" (e.g. "I recommend you end the session") would incorrectly trigger session termination.

### 5. Structured audit logging to file

**Decision**: Audit entries are written as newline-delimited JSON to a file in the application's data directory, with no chain hashing in this iteration. Each entry records: timestamp (UTC), session ID, event type, sanitised input/output (truncated to 512 chars), and policy decision.

**Rationale**: Chain hashing provides tamper evidence but adds complexity. For the initial implementation, structured JSON to file is sufficient to address ASI-06. Chain hashing can be added as a follow-on.

**Alternatives considered**:
- *Structured logging via `ILogger`*: ASP.NET ILogger is write-ahead, not append-only to a dedicated file; it does not satisfy the "logs stored separately from agent-writable directories" requirement
- *Database table*: adds a storage dependency not currently in the project

### 6. Circuit breaker via `Microsoft.Extensions.Http.Resilience`

**Decision**: The `OllamaClient` HTTP client uses the Polly-backed `AddStandardResilienceHandler()` configured with a circuit breaker (5 failures → 30 s open) and a per-session rate limit (10 requests/minute).

**Rationale**: `Microsoft.Extensions.Http.Resilience` is the Microsoft-recommended library for resilience pipelines; it is already transitively available in .NET 9. Adding Polly directly would duplicate the dependency.

## Risks / Trade-offs

- **False positives in input validation** → Mitigation: conservative patterns; validation failures return a user-visible "blocked by policy" message rather than silently discarding input, so users can rephrase
- **Audit log growth** → Mitigation: log rotation is out of scope for this change; document that operators must configure log retention externally
- **Performance overhead of validation on every message** → Mitigation: all checks are in-memory regex; measured overhead is expected to be <1 ms per call
- **Circuit breaker trips during Ollama restart** → Mitigation: 30 s open window is short; the application displays an "unavailable" message already (per existing ASI-04 requirement)
- **ASI-07 and ASI-09 remain open** → Accepted: documented as deferred; the remaining 6 closures bring the application from HIGH to MEDIUM risk

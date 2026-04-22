## 1. Interface Layer — Input Validation (Anamnesis.Interface.Website)

- [ ] 1.1 Add a `MaxMessageLength` constant (4 096) to the symptom input component in `Anamnesis.Interface.Website`
- [ ] 1.2 Extend the existing empty-check in the form submission handler to also reject inputs exceeding `MaxMessageLength`
- [ ] 1.3 Add compiled regex checks in the form submission handler for common prompt injection patterns (e.g. "ignore previous instructions", "you are now", "disregard your system prompt"); reject matching inputs with a user-visible message
- [ ] 1.4 Wrap the validation block in try/catch; treat any exception as a validation failure (fail closed) and display a generic blocked message

## 2. Use Case Layer — Policy Enforcement (Anamnesis.UseCase.Conversation)

- [ ] 2.1 Add a `PolicyEvaluator` internal class to `Anamnesis.UseCase.Conversation` with in-process code predicates returning `Allow` or `Deny`; no LLM call
- [ ] 2.2 Wrap all predicates in try/catch; return `Deny` on any exception (fail closed)
- [ ] 2.3 Call `PolicyEvaluator.Evaluate` at the start of `ConversationService.SendAsync` after the message is received from the use case boundary; return a policy-blocked response string if `Deny`

## 3. Use Case Layer — LLM Response Integrity (Anamnesis.UseCase.Conversation)

- [ ] 3.1 Replace `Regex.IsMatch(response.Trim(), @"\bEND\b")` in `ConversationService.CheckContinuationAsync` with a strict equality check: only `"END"` (case-insensitive, trimmed) ends the session; any other value is treated as `CONTINUE`

## 4. Use Case Layer — Audit Logging (Anamnesis.UseCase.Conversation)

- [ ] 4.1 Define `IAuditLogger` interface in `Anamnesis.UseCase.Conversation.Contract` with `LogAsync(AuditEntry entry)` method
- [ ] 4.2 Define `AuditEntry` record in `Anamnesis.UseCase.Conversation.Contract` with `Timestamp` (DateTimeOffset), `SessionId` (string), `EventType` (string), and `Detail` (string, max 512 chars)
- [ ] 4.3 Implement `FileAuditLogger` in `Anamnesis.UseCase.Conversation` writing newline-delimited JSON to a file in the application data directory; open in append mode
- [ ] 4.4 Inject `IAuditLogger` into `ConversationService` via constructor
- [ ] 4.5 Add `session_start` audit entry when `ConversationService` initialises a new session
- [ ] 4.6 Add `user_message` audit entry in `SendAsync` after validation and policy pass (before LLM call)
- [ ] 4.7 Add `llm_response` audit entry in `SendAsync` after the LLM responds
- [ ] 4.8 Add `policy_deny` audit entry in `SendAsync` when policy returns `Deny`
- [ ] 4.9 Add `session_end` audit entry when the session ends (user or LLM-initiated)
- [ ] 4.10 Register `IAuditLogger` / `FileAuditLogger` in `Program.cs` as a scoped service; configure `AuditLogging:FilePath` in `appsettings.json`

## 5. Adapter Layer — Circuit Breaker and Rate Limiting (Anamnesis.Adapter.Ollama)

- [ ] 5.1 Add `Microsoft.Extensions.Http.Resilience` NuGet package to `Anamnesis.Adapter.Ollama`
- [ ] 5.2 Configure `AddStandardResilienceHandler()` on the `OllamaClient` HTTP client in `Program.cs` with a circuit breaker: 5 failures → 30 s open window
- [ ] 5.3 Add a per-session rate limiter (10 requests/minute) to the `OllamaClient` resilience pipeline
- [ ] 5.4 Define `RateLimitExceededException` in `Anamnesis.Adapter.Ollama.Contract`
- [ ] 5.5 Handle `RateLimitExceededException` and `OllamaUnavailableException` in `ConversationService` to surface a user-visible error without crashing

## 6. Adapter Layer — Ollama Hardening (Anamnesis.Adapter.Ollama)

- [ ] 6.1 Retain `Model` in `OllamaSettings` and ensure it is always read from `appsettings.json`; add a startup validation that the configured value starts with `"medgemma"` (case-insensitive) and throw `InvalidOperationException` if it does not
- [ ] 6.2 Add a constructor-time assertion in `OllamaClient` that the configured `BaseUrl` targets only the `/api/chat` path (no other Ollama endpoints are callable)
- [ ] 6.3 Add an upper bound guard in `OllamaClient.ChatAsync`: throw `ArgumentException` if the message history exceeds 200 entries

## 7. Tests

- [ ] 7.1 Add unit tests in `Anamnesis.UseCase.Conversation.Test` for `PolicyEvaluator`: allows valid input, denies matched input, fails closed on exception
- [ ] 7.2 Add unit tests in `Anamnesis.UseCase.Conversation.Test` for `ConversationService.CheckContinuationAsync`: `"END"` returns false, `"CONTINUE"` returns true, any unrecognised value returns true
- [ ] 7.3 Add unit tests in `Anamnesis.UseCase.Conversation.Test` for `FileAuditLogger`: entry written with all required fields, `Detail` truncated at 512 chars, file opened in append mode
- [ ] 7.4 Add unit tests in `Anamnesis.Adapter.Ollama.Test` for the message count guard: throws when history exceeds 200 entries

## Context

`ConversationService` (use case layer) currently owns `PromptTemplates.cs`, manages conversation history as a `List<ConversationMessage>` (LLM role/content records), parses LLM response strings (continuation check, sidebar JSON), and references `IOllamaClient` from the adapter contract. This violates the Ports & Adapters pattern — the use case should depend only on its own abstractions, not on adapter-layer types.

The existing `IOllamaClient` takes `IEnumerable<ConversationMessage>` — a raw, LLM-shaped data structure. The use case builds that list, embeds prompts into it, and interprets raw string responses. The adapter boundary currently offers too low a level of abstraction.

## Goals / Non-Goals

**Goals:**
- Define `IConversationEngine` in `Anamnesis.UseCase.Conversation.Contract` as a domain-level port with four operations: send message, check continuation, summarise, get related conditions.
- Remove all prompt, history, and response-parsing logic from `ConversationService`.
- Create `OllamaConversationEngine` in `Anamnesis.Adapter.Ollama` that implements `IConversationEngine`, wraps `IOllamaClient`, and owns all LLM concerns.
- Delete `PromptTemplates.cs` from the use case project.
- Preserve all existing external behaviour and audit-logging semantics exactly.

**Non-Goals:**
- Changing the Ollama HTTP communication layer (`OllamaClient`/`IOllamaClient`).
- Altering any user-facing feature or conversation flow.
- Supporting multiple simultaneous `IConversationEngine` implementations (e.g., OpenAI); that is a future concern.
- Modifying the policy evaluation (`PolicyEvaluator`) or audit logging (`FileAuditLogger`).

## Decisions

### D1: `IConversationEngine` owns conversation history (stateful per-session)

**Decision**: The engine is stateful — it manages the `List<ConversationMessage>` internally rather than the use case passing history on each call.

**Rationale**: The use case does not need to know what a "conversation history" looks like at the LLM level. If the engine were stateless, the use case would still need to maintain a history structure and pass it in, which leaks the concept. Making the engine stateful per-session (scoped DI lifetime) matches how `ConversationService` is already scoped.

**Alternative considered**: Stateless engine accepting a domain-level `IReadOnlyList<ConversationTurn>`. Rejected because it introduces a new domain type (`ConversationTurn`) that serves no purpose except to hide the LLM shape — extra indirection for no gain.

### D2: `IConversationEngine` is defined in the use case contract layer

**Decision**: `IConversationEngine` lives in `Anamnesis.UseCase.Conversation.Contract`, not in a new shared project or in the adapter contract.

**Rationale**: The port belongs to the consumer (the use case), not the provider (the adapter). The adapter project references the use case contract to implement the interface — this is the correct dependency direction in Ports & Adapters. The Ollama adapter already references `Anamnesis.Domain`; adding a reference to `Anamnesis.UseCase.Conversation.Contract` follows the same pattern.

**Alternative considered**: A new `Anamnesis.Ports` project. Rejected as unnecessary indirection for a single interface.

### D3: `IOllamaClient` is preserved as an internal seam

**Decision**: `IOllamaClient` is not removed. `OllamaConversationEngine` depends on it for HTTP communication; unit tests can mock it.

**Rationale**: Keeping the HTTP boundary separately testable is valuable. `OllamaConversationEngine` tests mock `IOllamaClient`; integration tests can hit a real Ollama instance.

**Alternative considered**: Collapsing `OllamaConversationEngine` and `OllamaClient` into a single class. Rejected because it makes unit-testing prompt logic harder (would need to mock HTTP).

### D4: `PromptTemplates.cs` moves wholesale into `Anamnesis.Adapter.Ollama`

**Decision**: Move the file as-is (access modifier changed to `internal`), then modify as needed.

**Rationale**: Keeping prompts together and colocated with the engine that uses them maximises cohesion. Making it `internal` enforces that prompts are an adapter detail.

### D5: NHS name lists are passed as parameters to `GetRelatedConditionsAsync`

**Decision**: `IConversationEngine.GetRelatedConditionsAsync` accepts `IEnumerable<string> conditionNames, IEnumerable<string> symptomNames`. The use case resolves these from `INhsIndexService` and hands them to the engine.

**Rationale**: The engine should not depend on `INhsIndexService`; that is a use-case-layer dependency. The use case already knows how to call `INhsIndexService`, so it provides the names. The engine's job is to build and execute the LLM prompt — it only needs the string lists. This keeps the engine testable without an NHS index.

### D6: Exception surface unchanged

**Decision**: `OllamaConversationEngine` allows `OllamaUnavailableException` and `RateLimitExceededException` to propagate unchanged. The use case does not catch them; they surface to the UI layer as before.

**Rationale**: The UI already handles these; changing the exception contract would require UI changes outside scope.

## Risks / Trade-offs

- **Scoped engine lifetime required** → The engine holds conversation history in memory; it must be registered as `Scoped` (matching `ConversationService`). Accidentally registering as `Singleton` would cross-contaminate sessions. Mitigation: add a comment in `Program.cs` and verify via existing session-isolation tests.

- **Adapter references use-case contract** → `Anamnesis.Adapter.Ollama` will reference `Anamnesis.UseCase.Conversation.Contract`. This is correct for Ports & Adapters but is a new project dependency. Mitigation: document in design; enforce via ArchUnit rule (already planned in README).

- **`OllamaConversationEngine` unit tests must be updated/added** → Current `OllamaClientTests` test HTTP behaviour; new tests are needed for prompt/parsing logic. Mitigation: add `OllamaConversationEngineTests` as part of this change.

## Migration Plan

1. Add `IConversationEngine` to `Anamnesis.UseCase.Conversation.Contract`.
2. Add `OllamaConversationEngine` to `Anamnesis.Adapter.Ollama` (moves prompts, history, parsing).
3. Update `ConversationService` to depend on `IConversationEngine`; remove prompt and history code.
4. Delete `PromptTemplates.cs` from use case project.
5. Update `Program.cs` DI registration.
6. Update `ConversationServiceTests` to mock `IConversationEngine`.
7. Add `OllamaConversationEngineTests`.

No database migrations, feature flags, or deployment coordination required — this is a pure refactoring with no external surface change. Rollback is a git revert.

## Open Questions

- None. The scope and approach are clear.

## Context

`Anamnesis.Adapter.Ollama.Contract` currently contains three types:
- `IOllamaClient` — an internal seam between `OllamaConversationEngine` and `OllamaClient`
- `OllamaUnavailableException` — thrown when the Ollama HTTP client cannot reach the server
- `RateLimitExceededException` — thrown when the rate limiter is exhausted

The project name and namespace expose Ollama by name at the contract boundary, which is the same problem that prompted the rename of `Adapter.Nhs.Contract` → `Adapter.MedicalData.Contract`. The contract should declare the category of adapter (LLM), not the implementation (Ollama).

Additionally, `IOllamaClient` is a purely internal seam — only used by `OllamaConversationEngine` to call `OllamaClient`. It is not part of a contract that any other layer should depend on; it belongs inside the Ollama adapter, not in its contract project.

## Goals / Non-Goals

**Goals:**
- Create `Anamnesis.Adapter.Llm.Contract` with generic LLM exception types
- Rename `OllamaUnavailableException` → `LlmUnavailableException`
- Move `IOllamaClient` into `Anamnesis.Adapter.Ollama` as an internal interface
- Delete `Anamnesis.Adapter.Ollama.Contract`
- Update all references throughout the solution

**Non-Goals:**
- Changing `IConversationEngine` — it already lives in `UseCase.Conversation.Contract` and is correctly technology-agnostic
- Moving `ConversationMessage` out of `Domain` — a separate concern
- Changing any runtime behaviour of the Ollama adapter

## Decisions

### 1. `IOllamaClient` becomes internal to `Adapter.Ollama`

`IOllamaClient` is an implementation detail of the Ollama adapter — it is the seam that allows `OllamaConversationEngine` and `OllamaClient` to be tested independently. No other layer references it (after `Infrastructure` was extracted, `IOllamaClient` is accessed only within the Ollama stack). Making it internal matches the intent: it is not a port the wider system cares about.

`Adapter.Ollama.csproj` gains `InternalsVisibleTo` pointing to `Adapter.Ollama.Test`, allowing the test project to mock `IOllamaClient` with NSubstitute as before.

Alternative considered: keep `IOllamaClient` in a separate `Adapter.Ollama.Contract` alongside the new `Adapter.Llm.Contract`. Rejected — this replicates the same naming problem for the internal seam and adds an extra project with minimal benefit.

### 2. `OllamaUnavailableException` → `LlmUnavailableException`

The exception name must match the contract project name. A caller catching `LlmUnavailableException` knows the LLM adapter failed without knowing it was Ollama. This is the correct abstraction: the use case (or infrastructure) handles "LLM unavailable", not "Ollama unavailable".

`RateLimitExceededException` requires no rename — it already describes a generic condition.

### 3. `Adapter.Ollama.Contract` is deleted outright

There is nothing Ollama-specific that belongs in a public contract. The exceptions migrate to `Adapter.Llm.Contract` under new names; `IOllamaClient` migrates into the adapter itself. The empty husk project is removed.

## Risks / Trade-offs

- [Test access to internals] Making `IOllamaClient` internal requires `InternalsVisibleTo` in the production project. → Mitigation: this is the established pattern already used by `UseCase.Conversation` → `UseCase.Conversation.Test`.
- [Exception rename] Any caller catching `OllamaUnavailableException` by name will break at compile time. → All call sites are within this solution and will be updated as part of this change; no public API surface is exposed.

## Migration Plan

1. Create `Adapter.Llm.Contract` with `LlmUnavailableException` and `RateLimitExceededException`; add to solution
2. Move `IOllamaClient` into `Adapter.Ollama`; add `InternalsVisibleTo` for test project
3. Update `Adapter.Ollama.csproj`: replace `Adapter.Ollama.Contract` reference with `Adapter.Llm.Contract`; update all `using` and exception references in `.cs` files
4. Update `Adapter.Ollama.Test.csproj`: replace reference; update `using` and exception references in test files
5. Update `Infrastructure.csproj`: replace reference; update `using`
6. Delete `Adapter.Ollama.Contract`; remove from solution
7. Build and test

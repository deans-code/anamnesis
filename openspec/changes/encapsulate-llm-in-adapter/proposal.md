## Why

The use case layer (`ConversationService`) currently owns LLM-specific concerns — prompt definitions, system prompt insertion, conversation history as LLM role/content messages, and response parsing — which couples it to Ollama's implementation model and violates the Ports & Adapters boundary. Moving these concerns into the Ollama adapter makes the use case technology-agnostic, so it can express *what* a conversation accomplishes without knowing *how* the LLM is invoked.

## What Changes

- **New**: `IConversationEngine` interface added to the use case contract, defining domain-level conversation operations (send message, check continuation, summarise, get related conditions).
- **Removed**: `IOllamaClient` dependency removed from `ConversationService`; use case no longer references any Ollama adapter contract.
- **Moved**: All prompt definitions (`PromptTemplates.cs`) moved from the use case project into the Ollama adapter.
- **Moved**: Conversation history management (as LLM `ConversationMessage` records) moved from `ConversationService` into the adapter.
- **Moved**: LLM response parsing (`ParseSidebarResponse`, continuation value parsing) moved into the adapter.
- **New**: `OllamaConversationEngine` class created in the adapter, implementing `IConversationEngine` and wrapping `IOllamaClient`.
- **Simplified**: `ConversationService` retains only audit logging, policy evaluation, session-state tracking, and delegation to `IConversationEngine`.
- **Updated**: DI registration in `Program.cs` wires `OllamaConversationEngine` to `IConversationEngine`.
- **Updated**: Unit tests updated to mock `IConversationEngine` rather than `IOllamaClient` in conversation service tests.

## Capabilities

### New Capabilities

- `conversation-engine-port`: Architectural boundary — the `IConversationEngine` port that decouples the use case from LLM technology. Specifies the contract the use case depends on and the constraints any implementation must satisfy.

### Modified Capabilities

- none (external user-facing behaviour is unchanged)

## Impact

- `Anamnesis.UseCase.Conversation` — `ConversationService.cs` simplified; `PromptTemplates.cs` deleted.
- `Anamnesis.UseCase.Conversation.Contract` — `IConversationEngine` interface added.
- `Anamnesis.Adapter.Ollama` — `OllamaConversationEngine.cs` added; prompt and history logic moved here.
- `Anamnesis.Adapter.Ollama.Contract` — `IOllamaClient` remains as an internal Ollama HTTP contract; `OllamaUnavailableException` and `RateLimitExceededException` remain here.
- `Anamnesis.Interface.Website/Program.cs` — DI registration updated.
- `Anamnesis.UseCase.Conversation.Test` — Tests updated to mock `IConversationEngine`.

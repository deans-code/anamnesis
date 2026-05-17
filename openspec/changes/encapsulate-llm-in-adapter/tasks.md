## 1. Define the IConversationEngine port

- [x] 1.1 Add `IConversationEngine` interface to `Anamnesis.UseCase.Conversation.Contract` with methods: `SendMessageAsync(string userMessage)`, `CheckShouldContinueAsync()`, `SummariseAsync()`, and `GetRelatedConditionsAsync(IEnumerable<string> conditionNames, IEnumerable<string> symptomNames)` returning `Task<string>`, `Task<bool>`, `Task<string>`, and `Task<SidebarResult>` respectively.

## 2. Create OllamaConversationEngine

- [x] 2.1 Add a project reference from `Anamnesis.Adapter.Ollama` to `Anamnesis.UseCase.Conversation.Contract` in the `.csproj`.
- [x] 2.2 Move `PromptTemplates.cs` from `Anamnesis.UseCase.Conversation` into `Anamnesis.Adapter.Ollama`; change its access modifier to `internal`.
- [x] 2.3 Create `OllamaConversationEngine` in `Anamnesis.Adapter.Ollama` implementing `IConversationEngine`. Give it a constructor that accepts `IOllamaClient`. Initialise an internal `List<ConversationMessage> _history`.
- [x] 2.4 Implement `SendMessageAsync`: prepend the system prompt on the first call, append the user message, call `IOllamaClient.ChatAsync(_history)`, append the assistant reply to history, and return the reply.
- [x] 2.5 Implement `CheckShouldContinueAsync`: build a temporary message list (`_history` + continuation-check prompt), call `IOllamaClient.ChatAsync`, parse the trimmed response to `bool` (false for "END", true for anything else), and return the result without modifying `_history`.
- [x] 2.6 Implement `SummariseAsync`: build a temporary message list (`_history` + summary prompt), call `IOllamaClient.ChatAsync`, and return the result without modifying `_history`.
- [x] 2.7 Implement `GetRelatedConditionsAsync`: build a temporary message list (`_history` + sidebar prompt constructed from the provided condition and symptom name lists), call `IOllamaClient.ChatAsync`, parse the JSON response into `SidebarResult`, and return it without modifying `_history`. Handle malformed JSON by returning an empty `SidebarResult`.

## 3. Refactor ConversationService

- [x] 3.1 Replace the `IOllamaClient` constructor parameter in `ConversationService` with `IConversationEngine`.
- [x] 3.2 Remove `_history`, `_systemPromptSent`, and all history-management code from `ConversationService`.
- [x] 3.3 Update `SendAsync` to call `_engine.SendMessageAsync(userMessage)` and use the returned string as the assistant reply; preserve audit logging and policy evaluation around the call.
- [x] 3.4 Update `CheckContinuationAsync` to call `_engine.CheckShouldContinueAsync()` and return the bool result.
- [x] 3.5 Update `RequestSummaryAsync` to call `_engine.SummariseAsync()` and return the string result.
- [x] 3.6 Update `GetRelatedConditionsAsync` to resolve condition and symptom names from `INhsIndexService`, then call `_engine.GetRelatedConditionsAsync(conditionNames, symptomNames)` and return the result.
- [x] 3.7 Remove the `using` reference to `Anamnesis.Adapter.Ollama.Contract` from `ConversationService.cs` and its project `.csproj`.
- [x] 3.8 Delete `PromptTemplates.cs` from `Anamnesis.UseCase.Conversation`.

## 4. Update dependency injection

- [x] 4.1 In `Program.cs`, register `OllamaConversationEngine` as the `IConversationEngine` implementation with a `Scoped` lifetime.
- [x] 4.2 Verify the existing `IOllamaClient` / `OllamaClient` registration is unchanged (still used internally by `OllamaConversationEngine`).
- [x] 4.3 Remove any now-unused `using` directives or service registrations related to the old `IOllamaClient`→`ConversationService` wiring.

## 5. Update and add tests

- [x] 5.1 Update `ConversationServiceTests` to inject a mock `IConversationEngine` instead of `IOllamaClient`; update all test assertions accordingly.
- [x] 5.2 Add `OllamaConversationEngineTests` in `Anamnesis.Adapter.Ollama.Test`: mock `IOllamaClient` and verify that (a) the system prompt is prepended only on the first `SendMessageAsync` call, (b) continuation/summary/sidebar calls do not mutate `_history`, and (c) `GetRelatedConditionsAsync` returns an empty result on malformed JSON.
- [x] 5.3 Run the full test suite and confirm all tests pass.

## ADDED Requirements

### Requirement: Use case communicates with LLM via a technology-agnostic port
The system SHALL define an `IConversationEngine` interface in the use case contract layer (`Anamnesis.UseCase.Conversation.Contract`). The interface SHALL expose four domain-level operations: sending a user message and receiving an assistant reply, checking whether the conversation should continue, requesting a conversation summary, and requesting related conditions and symptoms. The use case layer SHALL NOT reference any type from the Ollama adapter contract (`Anamnesis.Adapter.Ollama.Contract`) or any LLM-specific concept such as message roles, prompt strings, or raw LLM response formats.

#### Scenario: Use case project has no Ollama adapter dependency
- **WHEN** the `Anamnesis.UseCase.Conversation` project is compiled
- **THEN** its project references SHALL NOT include `Anamnesis.Adapter.Ollama` or `Anamnesis.Adapter.Ollama.Contract`

#### Scenario: Sending a user message returns an assistant reply
- **WHEN** `IConversationEngine.SendMessageAsync(userMessage)` is called with a non-empty user message
- **THEN** the engine SHALL return the assistant's response as a plain string, managing all LLM history, prompt construction, and response handling internally

#### Scenario: Continuation check returns a boolean
- **WHEN** `IConversationEngine.CheckShouldContinueAsync()` is called
- **THEN** the engine SHALL return `true` if the conversation should continue and `false` if it should end, with all LLM prompt construction and response parsing handled internally by the engine

#### Scenario: Summary request returns a string
- **WHEN** `IConversationEngine.SummariseAsync()` is called
- **THEN** the engine SHALL return the LLM-generated summary as a plain string, with all prompt construction handled internally

#### Scenario: Related conditions request accepts NHS name lists
- **WHEN** `IConversationEngine.GetRelatedConditionsAsync(conditionNames, symptomNames)` is called with lists of NHS condition and symptom names
- **THEN** the engine SHALL return a `SidebarResult` containing matched conditions and symptoms, with all prompt construction and JSON parsing handled internally

### Requirement: All LLM concerns are encapsulated within the adapter
All prompt definitions, conversation history management (including system prompt insertion), and LLM response parsing SHALL reside exclusively in the Ollama adapter project (`Anamnesis.Adapter.Ollama`). No prompt string, LLM message role value, or LLM response parsing logic SHALL exist in the use case or domain layers.

#### Scenario: PromptTemplates does not exist in the use case project
- **WHEN** the `Anamnesis.UseCase.Conversation` project is inspected
- **THEN** it SHALL contain no class named `PromptTemplates` and no string literals representing LLM prompts

#### Scenario: Conversation history is not visible to the use case
- **WHEN** `ConversationService` sends a message via `IConversationEngine`
- **THEN** `ConversationService` SHALL NOT construct, read, or modify any list of `ConversationMessage` objects; history management is the engine's responsibility

### Requirement: OllamaConversationEngine implements IConversationEngine
The Ollama adapter SHALL provide `OllamaConversationEngine`, a scoped, stateful class that implements `IConversationEngine`. It SHALL maintain the conversation history as a `List<ConversationMessage>`, apply the medical system prompt on the first message, and delegate all HTTP communication to `IOllamaClient`.

#### Scenario: System prompt is applied on first message only
- **WHEN** `SendMessageAsync` is called for the first time in a session
- **THEN** `OllamaConversationEngine` SHALL prepend a system-role `ConversationMessage` containing the medical assistant prompt before sending history to `IOllamaClient`

#### Scenario: Continuation check does not persist in history
- **WHEN** `CheckShouldContinueAsync` is called
- **THEN** the continuation-check prompt and the LLM's response SHALL NOT be added to the persistent conversation history used for subsequent `SendMessageAsync` calls

#### Scenario: Summary does not persist in history
- **WHEN** `SummariseAsync` is called
- **THEN** the summary prompt and the LLM's response SHALL NOT be added to the persistent conversation history

#### Scenario: Sidebar call does not persist in history
- **WHEN** `GetRelatedConditionsAsync` is called
- **THEN** the sidebar prompt and the LLM's response SHALL NOT be added to the persistent conversation history

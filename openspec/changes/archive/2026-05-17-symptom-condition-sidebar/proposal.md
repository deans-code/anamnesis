## Why

During the symptom exploration conversation, the application gathers increasingly specific clinical detail but provides no visible feedback about what that information might relate to. Adding a live sidebar that surfaces potentially related conditions and symptoms — with links to authoritative NHS pages — demonstrates the AI's reasoning capability and gives users useful context to bring to their healthcare provider.

## What Changes

- A sidebar panel appears alongside the chat interface once the conversation has started
- After each assistant response, a background LLM call analyses the conversation and returns a structured list of potentially related conditions and symptoms
- Each entry links to the relevant NHS A-to-Z conditions page (`https://www.nhs.uk/health-a-to-z/conditions/`) or symptoms page (`https://www.nhs.uk/symptoms/`)
- A persistent, prominent disclaimer states that the suggestions are not a diagnosis and exist solely to demonstrate AI capability
- The sidebar updates progressively as the conversation deepens

## Capabilities

### New Capabilities

- `condition-sidebar`: Live sidebar panel that analyses the in-progress symptom conversation and surfaces potentially related NHS conditions and symptoms with links and a non-diagnostic disclaimer

### Modified Capabilities

- `llm-conversation`: An additional background LLM call is made after each assistant response to populate the sidebar; this extends the conversation flow without altering the core symptom-gathering behaviour

## Impact

- `Home.razor`: Layout extended to a two-column design (chat + sidebar) once the conversation starts
- `ConversationService` / `IConversationService`: New method for the sidebar analysis call
- `PromptTemplates`: New prompt for extracting related conditions/symptoms as structured data
- No new dependencies; uses the existing `IOllamaClient`
- External links are hard-coded NHS URLs — no HTTP calls to NHS from the application

## ADDED Requirements

### Requirement: Background sidebar analysis call after each assistant response
After each successful assistant response is rendered, the system SHALL make a secondary background LLM call to analyse the conversation history. The sidebar analysis prompt SHALL include the full list of NHS condition names and symptom names from the cached index, and SHALL instruct the LLM to return only exact names from those lists that are relevant to the conversation. This call SHALL be non-blocking: the main chat input SHALL be re-enabled before the sidebar call completes. The call SHALL NOT be added to the persistent conversation history. If the sidebar call fails for any reason, the system SHALL silently suppress the error and continue the conversation normally.

#### Scenario: Sidebar call is made after main response is rendered
- **WHEN** the main assistant response has been added to the chat view
- **THEN** the system SHALL initiate the sidebar analysis call without waiting for it to complete before re-enabling user input

#### Scenario: Sidebar prompt includes NHS name lists
- **WHEN** the sidebar analysis call is built
- **THEN** the prompt SHALL contain the cached list of NHS condition names and the cached list of NHS symptom names, and SHALL instruct the LLM to select only exact names from those lists

#### Scenario: Sidebar call failure does not affect the conversation
- **WHEN** the sidebar analysis call throws an exception or returns malformed data
- **THEN** the system SHALL suppress the error, leave the chat in its current state, and SHALL NOT surface an error message related to the sidebar call

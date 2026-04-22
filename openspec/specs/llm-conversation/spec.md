## MODIFIED Requirements

### Requirement: LLM asks one follow-up question at a time
The system SHALL send the full conversation history to the LLM with a system prompt that instructs it to ask exactly one focused follow-up question per turn. The system SHALL not advance to the next question until the user has responded. Before sending any message to the LLM, the system SHALL run the user input through the input validation pipeline. If validation returns `Deny`, the system SHALL NOT call the LLM and SHALL display a user-visible blocked message instead.

#### Scenario: LLM produces a follow-up question
- **WHEN** the user submits a response (initial or subsequent) that passes input validation and policy evaluation
- **THEN** the system SHALL send the updated conversation history to the LLM, display an animated loading indicator while waiting, and render the full LLM question in the chat view once the response is received

#### Scenario: Loading indicator is shown while processing
- **WHEN** an LLM call is in progress
- **THEN** the system SHALL display an animated loading indicator in the chat area and disable all user input controls until the response is received

#### Scenario: User input is blocked by validation
- **WHEN** the user submits a message that fails input validation or policy evaluation
- **THEN** the system SHALL NOT call the LLM, SHALL display a user-visible message indicating the request was blocked, and SHALL re-enable input so the user can rephrase

### Requirement: System prompt constrains LLM to medical history assistant role
The system SHALL include a system prompt that instructs the LLM to act as a medical history assistant, to ask one question at a time, to build on previously stated symptoms, and to probe for onset, duration, severity, associated symptoms, and relieving/aggravating factors. The system prompt SHALL explicitly instruct the LLM not to provide diagnoses or medical advice.

#### Scenario: LLM stays on topic
- **WHEN** the LLM generates a response
- **THEN** the response SHALL be a follow-up question related to the user's stated symptoms or a structured summary (when requested)

### Requirement: System automatically ends the conversation when the LLM signals completion
After rendering each LLM follow-up question, the system SHALL make a secondary LLM call to check whether sufficient information has been gathered. The system SHALL verify the LLM's continuation response against a strict allowlist (`"END"` or `"CONTINUE"`, case-insensitive, trimmed) before acting on it. If the LLM returns any other value, the system SHALL treat it as `CONTINUE`. If the LLM signals `END`, the system SHALL automatically end the session and generate the summary.

#### Scenario: LLM signals conversation can end
- **WHEN** the continuation-check LLM call returns exactly `"END"` (case-insensitive, trimmed)
- **THEN** the system SHALL automatically disable all inputs and initiate summary generation

#### Scenario: LLM signals more questions are needed
- **WHEN** the continuation-check LLM call returns exactly `"CONTINUE"` (case-insensitive, trimmed)
- **THEN** the system SHALL re-enable user input and wait for the user's next reply

#### Scenario: LLM returns an unrecognised continuation value
- **WHEN** the continuation-check LLM call returns any value other than `"END"` or `"CONTINUE"`
- **THEN** the system SHALL treat it as `CONTINUE`, re-enable user input, and SHALL NOT end the session

### Requirement: User can end the conversation at any time
The system SHALL provide an "End Session" button that the user can activate at any point to terminate the question sequence and trigger summary generation.

#### Scenario: User ends session
- **WHEN** the user clicks the "End Session" button
- **THEN** the system SHALL disable further input and initiate the session summary generation

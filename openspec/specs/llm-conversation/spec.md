### Requirement: LLM asks one follow-up question at a time
The system SHALL send the full conversation history to the LLM with a system prompt that instructs it to ask exactly one focused follow-up question per turn. The system SHALL not advance to the next question until the user has responded.

#### Scenario: LLM produces a follow-up question
- **WHEN** the user submits a response (initial or subsequent)
- **THEN** the system SHALL send the updated conversation history to the LLM, display an animated loading indicator while waiting, and render the full LLM question in the chat view once the response is received

#### Scenario: Loading indicator is shown while processing
- **WHEN** an LLM call is in progress
- **THEN** the system SHALL display an animated loading indicator in the chat area and disable all user input controls until the response is received

### Requirement: System prompt constrains LLM to medical history assistant role
The system SHALL include a system prompt that instructs the LLM to act as a medical history assistant, to ask one question at a time, to build on previously stated symptoms, and to probe for onset, duration, severity, associated symptoms, and relieving/aggravating factors. The system prompt SHALL explicitly instruct the LLM not to provide diagnoses or medical advice.

#### Scenario: LLM stays on topic
- **WHEN** the LLM generates a response
- **THEN** the response SHALL be a follow-up question related to the user's stated symptoms or a structured summary (when requested)

### Requirement: System automatically ends the conversation when the LLM signals completion
After rendering each LLM follow-up question, the system SHALL make a secondary LLM call to check whether sufficient information has been gathered. If the LLM signals that the conversation can end, the system SHALL automatically end the session and generate the summary without requiring user action.

#### Scenario: LLM signals conversation can end
- **WHEN** the continuation-check LLM call returns an `END` signal
- **THEN** the system SHALL automatically disable all inputs and initiate summary generation

#### Scenario: LLM signals more questions are needed
- **WHEN** the continuation-check LLM call returns a `CONTINUE` signal
- **THEN** the system SHALL re-enable user input and wait for the user's next reply

### Requirement: User can end the conversation at any time
The system SHALL provide an "End Session" button that the user can activate at any point to terminate the question sequence and trigger summary generation.

#### Scenario: User ends session
- **WHEN** the user clicks the "End Session" button
- **THEN** the system SHALL disable further input and initiate the session summary generation

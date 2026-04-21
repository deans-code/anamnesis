## ADDED Requirements

### Requirement: LLM generates a structured symptom summary on session end
When the session ends (either by user action or automatic LLM-driven termination), the system SHALL send a final message to the LLM instructing it to produce a structured summary of all symptoms reported during the conversation. The summary SHALL be organised by symptom with noted attributes (onset, duration, severity, associated symptoms, relieving/aggravating factors where available). The full summary SHALL be displayed at once when the LLM response is received.

#### Scenario: Summary generated after user clicks End Session
- **WHEN** the user clicks "End Session"
- **THEN** the system SHALL disable all inputs, show the loading indicator, request the summary from the LLM, and display the result in a clearly distinct block when received

#### Scenario: Summary generated after automatic session end
- **WHEN** the system automatically ends the session due to the LLM signalling conversation completion
- **THEN** the system SHALL show the loading indicator, request the summary from the LLM, and display the result in a clearly distinct block when received

#### Scenario: Summary includes all reported symptoms
- **WHEN** the summary is generated
- **THEN** the summary SHALL reflect all symptoms and details provided by the user during the conversation

### Requirement: User can copy the session summary
The system SHALL provide a copy-to-clipboard button on the rendered session summary so the user can retain their symptom record.

#### Scenario: User copies the summary
- **WHEN** the user clicks the copy button on the session summary
- **THEN** the system SHALL copy the summary text to the clipboard and briefly confirm the action (e.g., button label changes to "Copied!")

### Requirement: Application displays a medical disclaimer
The system SHALL display a prominent disclaimer stating that the application is not a medical diagnostic tool and that users should consult a qualified healthcare professional for any medical concerns.

#### Scenario: Disclaimer is visible
- **WHEN** the application is loaded
- **THEN** the disclaimer SHALL be visible on the page at all times (e.g., in the footer or header)

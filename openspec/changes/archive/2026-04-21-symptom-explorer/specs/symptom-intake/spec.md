## ADDED Requirements

### Requirement: User can describe initial symptoms
The system SHALL present a text input area on page load where the user can describe their symptoms in free text. The system SHALL require a non-empty description before proceeding to the conversational phase.

#### Scenario: User submits initial symptoms
- **WHEN** the user types a symptom description and submits the form
- **THEN** the system SHALL display the user's description in the conversation view and initiate the LLM-driven follow-up question sequence

#### Scenario: User submits empty input
- **WHEN** the user attempts to submit with an empty or whitespace-only input
- **THEN** the system SHALL display a validation message and prevent submission

#### Scenario: Page load state
- **WHEN** the application is first loaded
- **THEN** the system SHALL display the symptom input form prominently with a clear prompt instructing the user to describe their symptoms

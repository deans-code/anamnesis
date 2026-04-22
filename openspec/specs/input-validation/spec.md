## ADDED Requirements

### Requirement: User input is validated before reaching the LLM
The system SHALL run every user-submitted message through an input validation pipeline before it is added to the conversation history or sent to the LLM. The pipeline SHALL check message length, detect prompt injection patterns, and return a `ValidationResult` indicating whether the message is allowed or denied. If the result is `Deny`, the system SHALL NOT send the message to the LLM and SHALL return a user-visible error response.

#### Scenario: Message passes validation
- **WHEN** a user submits a message that passes all validation checks
- **THEN** the system SHALL add the message to conversation history and proceed with the LLM call

#### Scenario: Message exceeds maximum length
- **WHEN** a user submits a message longer than 4 096 characters
- **THEN** the system SHALL deny the message without calling the LLM and SHALL return a response indicating that the input is too long

#### Scenario: Message contains a prompt injection pattern
- **WHEN** a user submits a message that matches a known prompt injection pattern (e.g. "ignore previous instructions", "you are now", "disregard your system prompt")
- **THEN** the system SHALL deny the message without calling the LLM and SHALL return a response indicating that the request was blocked

#### Scenario: Validation pipeline errors
- **WHEN** an exception is thrown inside the validation pipeline
- **THEN** the system SHALL treat the result as `Deny` (fail closed) and SHALL NOT proceed with the LLM call

### Requirement: Input validation is deterministic and does not involve the LLM
The input validation pipeline SHALL consist solely of in-process checks (length limits, compiled regex patterns). It SHALL NOT make any LLM call, external HTTP call, or database query.

#### Scenario: Validation completes within budget
- **WHEN** the validation pipeline runs
- **THEN** it SHALL complete in under 5 ms for any input up to the maximum length

## ADDED Requirements

### Requirement: All LLM interactions are written to a structured audit log
The system SHALL write a structured audit entry for every LLM tool call, user input submission, session lifecycle event (start, end), and policy decision. Entries SHALL be written as newline-delimited JSON to a dedicated audit log file. The log file SHALL be separate from ASP.NET application logs.

#### Scenario: User submits a message
- **WHEN** a user message is submitted
- **THEN** the system SHALL write an audit entry recording the UTC timestamp, session ID, event type `"user_message"`, and a sanitised excerpt of the input (truncated to 512 characters)

#### Scenario: LLM call completes
- **WHEN** the Ollama adapter returns a response
- **THEN** the system SHALL write an audit entry recording the UTC timestamp, session ID, event type `"llm_response"`, and a sanitised excerpt of the response (truncated to 512 characters)

#### Scenario: Policy denies a request
- **WHEN** the policy evaluator returns `Deny`
- **THEN** the system SHALL write an audit entry recording the UTC timestamp, session ID, event type `"policy_deny"`, and the reason for the denial

#### Scenario: Session starts
- **WHEN** a new conversation session is initialised
- **THEN** the system SHALL write an audit entry with event type `"session_start"` and the session ID

#### Scenario: Session ends
- **WHEN** a conversation session ends (user or LLM-initiated)
- **THEN** the system SHALL write an audit entry with event type `"session_end"` and the session ID

### Requirement: Audit log entries contain required fields
Every audit entry SHALL include: `timestamp` (ISO 8601 UTC), `sessionId` (opaque string), `eventType` (string), and `detail` (string). All personally identifiable information in `detail` SHALL be truncated to 512 characters maximum.

#### Scenario: Audit entry is well-formed
- **WHEN** any auditable event occurs
- **THEN** the written JSON entry SHALL contain all four required fields with non-null values

### Requirement: Audit log is append-only
The audit logger SHALL open the log file in append mode. It SHALL NOT truncate or overwrite existing log entries.

#### Scenario: Application restarts
- **WHEN** the application is restarted
- **THEN** audit entries from previous sessions SHALL remain in the log file and new entries SHALL be appended after them

## ADDED Requirements

### Requirement: Deterministic policy evaluation gates all LLM requests
The system SHALL evaluate every user request through a `PolicyEvaluator` before the LLM call. The evaluator SHALL use compiled code predicates — no LLM call, no external service. It SHALL return `Allow` or `Deny`. If any predicate throws an exception, the evaluator SHALL return `Deny` (fail closed).

#### Scenario: Request is allowed by policy
- **WHEN** a user message passes all policy predicates
- **THEN** the system SHALL proceed with the LLM call

#### Scenario: Request is denied by policy
- **WHEN** a user message is matched by a deny predicate
- **THEN** the system SHALL NOT call the LLM and SHALL return a policy-blocked response to the caller

#### Scenario: Policy evaluator throws an exception
- **WHEN** an unhandled exception occurs during policy evaluation
- **THEN** the system SHALL return `Deny` and SHALL NOT proceed with the LLM call

### Requirement: Policy evaluation is fail-closed and does not involve the LLM
The `PolicyEvaluator` SHALL operate entirely within the process using in-memory predicates. It SHALL complete evaluation in under 0.1 ms. It SHALL NOT call the LLM or any external service as part of evaluation.

#### Scenario: Evaluation performance budget is met
- **WHEN** the policy evaluator is invoked with any valid input
- **THEN** it SHALL return a result in under 0.1 ms

### Requirement: LLM response is verified before use in control flow
Before the application uses an LLM response to make a control-flow decision (e.g. session continuation), it SHALL verify that the response conforms to a known valid value set. Any response that does not match the expected value set SHALL be treated as the safest default.

#### Scenario: LLM returns a recognised continuation signal
- **WHEN** `CheckContinuationAsync` receives a response that is exactly `"END"` or `"CONTINUE"` (case-insensitive, trimmed)
- **THEN** the system SHALL interpret the signal correctly

#### Scenario: LLM returns an unrecognised continuation response
- **WHEN** `CheckContinuationAsync` receives any other response
- **THEN** the system SHALL treat it as `CONTINUE` (default safe) and SHALL NOT end the session

## ADDED Requirements

### Requirement: NHS adapter follows the three-project adapter pattern
The solution SHALL contain three projects for the NHS adapter: `Anamnesis.Adapter.MedicalData.Contract` (the technology-agnostic port definition), `Anamnesis.Adapter.Nhs` (the implementation), and `Anamnesis.Adapter.Nhs.Test` (the test project), mirroring the structure of the Ollama adapter.

#### Scenario: Medical data adapter contract project exists in the solution
- **WHEN** the solution file is inspected
- **THEN** `Anamnesis.Adapter.MedicalData.Contract` SHALL be registered as a project

#### Scenario: NHS test project exists in the solution
- **WHEN** the solution file is inspected
- **THEN** `Anamnesis.Adapter.Nhs.Test` SHALL be registered as a project

### Requirement: NhsConditionReferenceService behaviour is covered by unit tests
`Anamnesis.Adapter.Nhs.Test` SHALL contain tests for `NhsConditionReferenceService` covering successful HTML parsing, in-memory caching, and HTTP failure handling.

#### Scenario: Valid NHS HTML is parsed into RelatedCondition entries
- **WHEN** `GetConditionsAsync()` or `GetSymptomsAsync()` is called and the HTTP response contains valid NHS index HTML with matching anchor tags
- **THEN** the service SHALL return a non-empty list of `RelatedCondition` records with correct names and absolute URLs

#### Scenario: Second call returns cached result without HTTP request
- **WHEN** `GetConditionsAsync()` is called twice in succession
- **THEN** only one HTTP request SHALL be made; the second call SHALL return the cached list

#### Scenario: HTTP failure returns an empty list
- **WHEN** the HTTP request throws an exception
- **THEN** the service SHALL return an empty list and SHALL NOT throw

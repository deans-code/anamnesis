### Requirement: Use case retrieves conditions and symptoms via a technology-agnostic port
The system SHALL define an `IMedicalReferenceService` interface in `Anamnesis.Adapter.MedicalData.Contract`. The interface SHALL expose two methods: `GetConditionsAsync()` and `GetSymptomsAsync()`, each returning `Task<IReadOnlyList<RelatedCondition>>`. The use case layer SHALL NOT reference any NHS-specific type, enum, or string — including `INhsIndexService`, `NhsIndexType`, and any NHS URL.

#### Scenario: Use case project has no NHS-specific type references
- **WHEN** the `Anamnesis.UseCase.Conversation` project and its contract are compiled
- **THEN** neither project SHALL contain any identifier that includes the string "Nhs" or "NHS"

#### Scenario: Retrieving conditions returns a list of named entries with URLs
- **WHEN** `IMedicalReferenceService.GetConditionsAsync()` is called
- **THEN** the service SHALL return a list of `RelatedCondition` records, each with a non-empty `Name` and an optional `Url`

#### Scenario: Retrieving symptoms returns a list of named entries with URLs
- **WHEN** `IMedicalReferenceService.GetSymptomsAsync()` is called
- **THEN** the service SHALL return a list of `RelatedCondition` records, each with a non-empty `Name` and an optional `Url`

### Requirement: All NHS-specific data-source concerns are encapsulated in the NHS adapter
All logic for fetching, parsing, and caching data from the NHS website SHALL reside exclusively in `Anamnesis.Adapter.Nhs`. No NHS URL, NHS HTML-parsing logic, or NHS-specific configuration SHALL exist in the use case or domain layers.

#### Scenario: NHS adapter project is the only location for NHS-specific code
- **WHEN** the solution is searched for references to "nhs.uk" or NHS HTML parsing
- **THEN** all such references SHALL be found only within `Anamnesis.Adapter.Nhs`

#### Scenario: Conditions are fetched from the NHS conditions index on first request
- **WHEN** `GetConditionsAsync()` is called and the cache is empty
- **THEN** the adapter SHALL fetch and parse the NHS conditions A–Z page and return the resulting entries

#### Scenario: Symptoms are fetched from the NHS symptoms index on first request
- **WHEN** `GetSymptomsAsync()` is called and the cache is empty
- **THEN** the adapter SHALL fetch and parse the NHS symptoms A–Z page and return the resulting entries

#### Scenario: Subsequent calls use the in-memory cache
- **WHEN** `GetConditionsAsync()` or `GetSymptomsAsync()` is called after a successful fetch
- **THEN** the adapter SHALL return the cached result without making an HTTP request

#### Scenario: Fetch failure returns an empty list
- **WHEN** the HTTP request to the NHS index page fails
- **THEN** the adapter SHALL log a warning and return an empty list, allowing the calling use case to continue gracefully

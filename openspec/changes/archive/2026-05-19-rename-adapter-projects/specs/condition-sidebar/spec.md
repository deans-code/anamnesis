## MODIFIED Requirements

### Requirement: System fetches and caches NHS index pages on first use
The system SHALL fetch the NHS conditions index page (`https://www.nhs.uk/health-a-to-z/conditions/`) and the NHS symptoms index page (`https://www.nhs.uk/symptoms/`) on first use and cache the parsed `(name, url)` entries in memory for the application lifetime. The cached name lists SHALL be injected into each sidebar analysis prompt so the LLM selects only from NHS-listed items. If either page cannot be fetched or parsed, the system SHALL log a warning and continue with an empty cache for that index; no exception SHALL be surfaced to the user. The implementation of this fetch-and-cache behaviour SHALL reside in `Anamnesis.Adapter.MedicalData.Nhs`, which implements `IMedicalReferenceService` from `Anamnesis.Adapter.MedicalData.Contract`. The conversation use case SHALL interact with the medical data source only through `IMedicalReferenceService` and SHALL NOT reference NHS types or the `Anamnesis.Adapter.MedicalData.Nhs` namespace.

#### Scenario: NHS index pages are successfully fetched and cached
- **WHEN** the application makes its first sidebar analysis call
- **THEN** the system SHALL have fetched both NHS index pages and built an in-memory lookup of condition names to NHS URLs and symptom names to NHS URLs

#### Scenario: NHS index page is unavailable at fetch time
- **WHEN** an NHS index page cannot be reached or returns an error response
- **THEN** the system SHALL log a warning, cache an empty index for that page, and continue; the sidebar SHALL show no entries for that index type

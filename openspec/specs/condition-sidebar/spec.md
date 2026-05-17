### Requirement: Sidebar displays potentially related conditions and symptoms
The system SHALL render a sidebar panel alongside the chat interface once the conversation has started. After each assistant response, the system SHALL make a background LLM call that analyses the conversation history and returns a list of exact condition and symptom names selected from the NHS index. The sidebar SHALL display each result as a link to the relevant NHS page. The sidebar SHALL accumulate results across turns, deduplicating by exact name, so that the same condition is not listed twice.

#### Scenario: Sidebar populates after first assistant response
- **WHEN** the assistant has responded to the user's initial symptom description
- **THEN** the system SHALL initiate a background sidebar analysis call and, upon completion, render any returned conditions and symptoms in the sidebar panel

#### Scenario: Sidebar updates after each subsequent reply
- **WHEN** the assistant responds to a follow-up user message
- **THEN** the system SHALL initiate a new sidebar analysis call and merge its results into the existing sidebar list, removing duplicates

#### Scenario: Sidebar shows loading state during analysis
- **WHEN** the sidebar analysis call is in progress
- **THEN** the sidebar SHALL display a loading indicator within the sidebar panel; the main chat input SHALL remain enabled

#### Scenario: Sidebar analysis returns no results
- **WHEN** the sidebar analysis call completes but the LLM returns an empty or unparseable result
- **THEN** the sidebar SHALL display no condition or symptom entries and SHALL NOT display an error to the user

#### Scenario: Sidebar analysis call fails
- **WHEN** the sidebar analysis call throws an exception
- **THEN** the system SHALL silently suppress the error; the sidebar retains its previous content and the main conversation continues unaffected

### Requirement: System fetches and caches NHS index pages on first use
The system SHALL fetch the NHS conditions index page (`https://www.nhs.uk/health-a-to-z/conditions/`) and the NHS symptoms index page (`https://www.nhs.uk/symptoms/`) on first use and cache the parsed `(name, url)` entries in memory for the application lifetime. The cached name lists SHALL be injected into each sidebar analysis prompt so the LLM selects only from NHS-listed items. If either page cannot be fetched or parsed, the system SHALL log a warning and continue with an empty cache for that index; no exception SHALL be surfaced to the user. The implementation of this fetch-and-cache behaviour SHALL reside in `Anamnesis.Adapter.Nhs`, which implements `IMedicalReferenceService` from `Anamnesis.Adapter.MedicalData.Contract`. The conversation use case SHALL interact with the medical data source only through `IMedicalReferenceService` and SHALL NOT reference NHS types or the `Anamnesis.Adapter.Nhs` namespace.

#### Scenario: NHS index pages are successfully fetched and cached
- **WHEN** the application makes its first sidebar analysis call
- **THEN** the system SHALL have fetched both NHS index pages and built an in-memory lookup of condition names to NHS URLs and symptom names to NHS URLs

#### Scenario: NHS index page is unavailable at fetch time
- **WHEN** an NHS index page cannot be reached or returns an error response
- **THEN** the system SHALL log a warning, cache an empty index for that page, and continue; the sidebar SHALL show no entries for that index type

### Requirement: NHS name lists are injected into each sidebar analysis prompt
The system SHALL include the full list of cached condition names and symptom names in the sidebar analysis prompt. The LLM SHALL be instructed to return only exact names from those lists. The application SHALL resolve each returned name to its NHS URL via direct lookup against the cached index.

#### Scenario: LLM returns exact names from the injected list
- **WHEN** the sidebar analysis call completes and the LLM returns names that exist in the cached index
- **THEN** the system SHALL resolve each name to its NHS URL via direct lookup and render each entry as a link

#### Scenario: LLM returns a name not present in the cached index
- **WHEN** the LLM returns a name that cannot be found in the cached index
- **THEN** the system SHALL silently drop that entry; it SHALL NOT be displayed in the sidebar

### Requirement: Each sidebar entry links to the relevant NHS page
The system SHALL render each condition or symptom entry as a hyperlink to its resolved NHS URL. Links SHALL open in a new browser tab. The link text SHALL be the name as returned by the LLM.

#### Scenario: Matched condition entry links to NHS conditions page
- **WHEN** a condition entry has been matched to an NHS URL under `https://www.nhs.uk/conditions/`
- **THEN** the sidebar SHALL render a link with that URL opening in a new tab (`target="_blank"`, `rel="noopener noreferrer"`)

#### Scenario: Matched symptom entry links to NHS symptoms page
- **WHEN** a symptom entry has been matched to an NHS URL under `https://www.nhs.uk/symptoms/`
- **THEN** the sidebar SHALL render a link with that URL opening in a new tab (`target="_blank"`, `rel="noopener noreferrer"`)

### Requirement: Sidebar displays a persistent non-diagnostic disclaimer
The system SHALL display a prominent, non-dismissible disclaimer at the top of the sidebar. The disclaimer SHALL state that the listed conditions and symptoms are not a diagnosis and are presented solely to demonstrate the capabilities of an AI model. The disclaimer SHALL remain visible at all times while the sidebar is shown.

#### Scenario: Disclaimer is visible when sidebar first appears
- **WHEN** the sidebar panel is rendered for the first time
- **THEN** the disclaimer SHALL be the first visible element in the sidebar and SHALL NOT have a close or dismiss control

#### Scenario: Disclaimer remains visible as sidebar content grows
- **WHEN** the sidebar list grows beyond the visible area
- **THEN** the disclaimer SHALL remain visible without scrolling (pinned or sticky at the top of the sidebar)

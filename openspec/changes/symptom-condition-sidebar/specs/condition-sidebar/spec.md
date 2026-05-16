## ADDED Requirements

### Requirement: Sidebar displays potentially related conditions and symptoms
The system SHALL render a sidebar panel alongside the chat interface once the conversation has started. After each assistant response, the system SHALL make a background LLM call that analyses the conversation history and returns a structured list of potentially related medical conditions and symptoms, each with a primary name and a list of synonyms. The sidebar SHALL display each result as a labelled entry, linked to the relevant NHS page when a match is found. The sidebar SHALL accumulate results across turns, deduplicating by resolved NHS URL for matched entries and by primary name for unmatched entries, so that the same condition is not listed twice.

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

### Requirement: System fetches and caches NHS index pages on startup
The system SHALL fetch the NHS conditions index page (`https://www.nhs.uk/health-a-to-z/conditions/`) and the NHS symptoms index page (`https://www.nhs.uk/symptoms/`) on first use and cache the parsed `(name, url)` entries in memory for the application lifetime. If either page cannot be fetched or parsed, the system SHALL log a warning and continue with an empty cache for that index; no exception SHALL be surfaced to the user.

#### Scenario: NHS index pages are successfully fetched and cached
- **WHEN** the application makes its first sidebar analysis call
- **THEN** the system SHALL have fetched both NHS index pages and built an in-memory lookup of condition names to NHS URLs and symptom names to NHS URLs

#### Scenario: NHS index page is unavailable at fetch time
- **WHEN** an NHS index page cannot be reached or returns an error response
- **THEN** the system SHALL log a warning, cache an empty index for that page, and continue; the sidebar SHALL display entries as plain text rather than links

### Requirement: System resolves NHS URLs by matching names and synonyms against the cached index
For each condition or symptom returned by the LLM, the system SHALL attempt to resolve a verified NHS URL by comparing the primary name and each synonym against the cached NHS index using case-insensitive string matching. The first matching index entry SHALL be used as the NHS URL for that result. If no match is found, the entry SHALL be displayed as plain text without a link.

#### Scenario: Primary name matches an NHS index entry
- **WHEN** the LLM returns a condition whose primary name matches an entry in the cached NHS conditions index (case-insensitive)
- **THEN** the sidebar SHALL render that entry as a link to the matched NHS URL

#### Scenario: Synonym matches an NHS index entry
- **WHEN** the LLM returns a condition whose primary name does not match but one of its synonyms matches an entry in the cached NHS conditions index (case-insensitive)
- **THEN** the sidebar SHALL render that entry as a link to the matched NHS URL, using the primary name as the link text

#### Scenario: No name or synonym matches the NHS index
- **WHEN** neither the primary name nor any synonym of a returned condition or symptom matches any entry in the cached NHS index
- **THEN** the sidebar SHALL display the primary name as plain text without a hyperlink

### Requirement: Each matched sidebar entry links to the relevant NHS page
The system SHALL render each condition or symptom entry that has a resolved NHS URL as a hyperlink. Links SHALL open in a new browser tab. The link text SHALL be the human-readable primary name returned by the LLM.

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

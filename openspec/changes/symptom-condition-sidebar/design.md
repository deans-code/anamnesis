## Context

The application currently renders a single-column layout: a full-width chat interface for the symptom conversation. The sidebar extends this to a two-column layout once the conversation starts. A second background LLM call is introduced after each assistant response to analyse the conversation and return a structured list of potentially related medical conditions and symptoms. The application then resolves a verified NHS URL for each result by matching the returned names and synonyms against the live NHS index pages (`https://www.nhs.uk/health-a-to-z/conditions/` and `https://www.nhs.uk/symptoms/`). The existing `ConversationService` and `IOllamaClient` are reused; an additional `HttpClient` is required for fetching NHS index pages.

## Goals / Non-Goals

**Goals:**
- Render a live sidebar alongside the chat that updates after each assistant response
- Surface potentially related conditions and symptoms with links to verified NHS pages
- Resolve NHS URLs by matching LLM-returned names and synonyms against the actual NHS index pages — not by trusting LLM-generated URLs
- Display a persistent, prominent non-diagnostic disclaimer in the sidebar
- Keep the sidebar call non-blocking so it does not delay the main conversation

**Non-Goals:**
- Personalised or ranked condition suggestions
- Mobile-optimised sidebar layout beyond basic stacking (out of scope for this iteration)
- Caching or deduplicating sidebar results across turns
- Verifying that matched NHS pages are currently reachable (dead-link checking)

## Decisions

### 1. Sidebar call is fire-and-forget relative to UI rendering

After the main assistant response is rendered, the sidebar call is awaited in the background without blocking the user from interacting. The UI shows a loading state inside the sidebar while the call is in progress.

**Alternatives considered:**
- Awaiting the sidebar call before re-enabling input: adds latency to every turn with no benefit to the main flow.
- Running sidebar and continuation checks in parallel (`Task.WhenAll`): feasible but complicates error handling; sequential (sidebar then continuation) is simpler and the latency difference is negligible.

### 2. LLM returns names and synonyms; app resolves URLs by matching against NHS index pages

The sidebar prompt instructs the LLM to return a JSON object with `conditions` and `symptoms` arrays. Each entry has a `name` (the primary term) and a `synonyms` array (alternate names, lay terms, and medical equivalents). The application does not trust LLM-generated URLs.

Instead, at startup the application fetches the two NHS index pages, parses the list of `(name, url)` entries from the HTML, and caches them in memory. When sidebar results arrive, each entry's `name` and all `synonyms` are matched case-insensitively against the cached index. If a match is found the NHS URL is used; if no match is found the entry is displayed as plain text without a link.

**Why synonyms:** NHS page titles use specific clinical or lay phrasing that may differ from what the LLM returns (e.g., LLM returns "heart attack"; NHS page is titled "Heart attack" with slug `/conditions/heart-attack/` — trivially matched, but "MI" or "myocardial infarction" as synonyms would also resolve it). Synonyms increase match rate without requiring the LLM to know NHS slugs.

**Alternatives considered:**
- LLM generates NHS URLs directly: LLM hallucination risk is high; a well-formed but wrong URL gives the user a confident-looking broken link.
- Slug derivation from LLM name (kebab-case lowercasing): brittle for multi-word conditions with irregular slugs.
- Linking only to the NHS index pages: loses specificity; user gets no targeted information.

### 3. NHS index pages fetched once at startup and cached in memory

A new `INhsIndexService` singleton fetches both NHS index pages (`conditions` and `symptoms`) on first use and caches the parsed `(name, url)` dictionaries for the application lifetime. A named `HttpClient` registered separately from the Ollama client is used for NHS requests.

Parsing strategy: extract all `<a>` elements whose `href` begins with `/conditions/` or `/symptoms/` respectively, using the link text as the name. This approach is straightforward and tolerant of minor NHS page layout changes.

**Risk:** NHS page structure changes could break parsing → mitigation: if parsing returns zero entries the service logs a warning and the sidebar shows no links (graceful degradation); no exception is surfaced to the user.

### 4. Sidebar logic added to `ConversationService` as a new method

A new `GetRelatedConditionsAsync()` method is added to `IConversationService` and `ConversationService`. It sends the conversation history plus the sidebar prompt to `IOllamaClient`, deserialises the JSON response into a raw result, calls `INhsIndexService` to resolve URLs for each entry, and returns a `SidebarResult`. The UI invokes it after each successful assistant response.

### 5. Two-column layout using MudBlazor Grid

Once `_hasStarted` is true, the page switches from a single `MudContainer` to a `MudGrid` with a chat column (8/12) and a sidebar column (4/12). On small screens MudBlazor's grid collapses these to full-width stacked panels.

### 6. Disclaimer rendered at the top of the sidebar, always visible

The non-diagnostic disclaimer is a `MudAlert` with `Severity.Warning` pinned at the top of the sidebar panel. It is never dismissible. Its text explicitly states the suggestions are not a diagnosis and are for AI capability demonstration only.

## Risks / Trade-offs

- **No synonym matches found for a result** → Entry is shown as plain text without a link; the sidebar remains useful as a list of named conditions even without links.
- **NHS index page unavailable at startup** → `INhsIndexService` returns empty caches; the sidebar renders conditions and symptoms as unlinked plain text. The main conversation is unaffected.
- **NHS page structure changes break parsing** → Service logs a warning and returns empty caches; graceful degradation to plain text.
- **Sidebar call increases Ollama load** → Each turn makes three LLM calls (main response, continuation check, sidebar). On slower hardware this may extend perceived latency. Mitigation: sidebar call is non-blocking.
- **JSON parse failure from LLM** → `GetRelatedConditionsAsync` returns an empty `SidebarResult`; the sidebar shows nothing rather than crashing.
- **Sidebar may display conditions that alarm the user** → The disclaimer and the medical-history-assistant framing mitigate this; the feature is explicitly positioned as an AI capability demonstration.

## Open Questions

- Should the sidebar accumulate conditions across turns (union of all turns) or only show the latest turn's result? (Proposed: accumulate, deduplicating by resolved NHS URL; unmatched entries deduplicated by name.)

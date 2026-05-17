## Context

The application currently renders a single-column layout: a full-width chat interface for the symptom conversation. The sidebar extends this to a two-column layout once the conversation starts. A second background LLM call is introduced after each assistant response to analyse the conversation. That call receives the full list of condition and symptom names from the NHS index pages as part of its prompt, and returns only exact names from those lists that are relevant to the conversation. The application then resolves the NHS URL for each returned name via a direct dictionary lookup against the cached index. The existing `ConversationService` and `IOllamaClient` are reused; an additional `HttpClient` is required for fetching NHS index pages.

## Goals / Non-Goals

**Goals:**
- Render a live sidebar alongside the chat that updates after each assistant response
- Surface potentially related conditions and symptoms with links to verified NHS pages
- Constrain the LLM to select only from NHS-listed conditions and symptoms — no free-form generation
- Resolve NHS URLs via exact dictionary lookup against the cached index — not by trusting LLM-generated URLs
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

### 2. NHS names injected into sidebar prompt; LLM selects exact names; app resolves URLs by direct lookup

The sidebar analysis prompt includes the full list of condition names and symptom names extracted from the cached NHS index pages. The LLM is instructed to return a JSON object with `conditions` and `symptoms` arrays containing only exact names from those lists that are relevant to the conversation. The application does not trust LLM-generated URLs.

Once the LLM returns exact names, the application performs a direct case-insensitive dictionary lookup against the cached index to retrieve each URL. Because the LLM is constrained to names that exist in the index, every returned entry resolves to a link; there are no plain-text fallback entries.

The sidebar prompt structure is:

```
CONDITIONS listed on the NHS website:
Abdominal aortic aneurysm, Acne, Alcohol-related liver disease, ...

SYMPTOMS listed on the NHS website:
Abdominal pain, Anxiety, Back pain, ...

[conversation history]

From the CONDITIONS and SYMPTOMS lists above, return a JSON object with two arrays —
"conditions" and "symptoms" — containing only exact names from the respective lists
that are relevant to this conversation. Return an empty array if nothing matches.
```

**Why this approach:** Semantic matching (synonyms, lay terms, clinical equivalents) is handled by the LLM where that capability belongs. URL resolution remains a trivial exact lookup in application code, eliminating the synonym-matching logic entirely. Sending names only (no URLs or descriptions) keeps the injected context small (~3–5 KB), which is acceptable for local Ollama models.

**Alternatives considered:**
- LLM generates NHS URLs directly: hallucination risk is high; a confident-looking broken link is worse than no link.
- App fuzzy-matches LLM free-form output against the index: synonym arrays in the LLM response and case-insensitive matching in the app — more complexity with lower match quality than letting the LLM do the matching itself.
- Injecting the list into the main conversation system prompt: pollutes the symptom-gathering context with data it does not need; kept separate so the main conversation remains focused.

### 3. NHS index pages fetched once on first use and cached in memory

A new `INhsIndexService` singleton fetches both NHS index pages (`conditions` and `symptoms`) on first use and caches the parsed `(name, url)` dictionaries for the application lifetime. A named `HttpClient` registered separately from the Ollama client is used for NHS requests.

Parsing strategy: extract all `<a>` elements whose `href` begins with `/conditions/` or `/symptoms/` respectively, using the link text as the name. This approach is straightforward and tolerant of minor NHS page layout changes.

The `INhsIndexService` interface exposes two concerns:
- `GetNames(NhsIndexType)` → `IReadOnlyList<string>` — returns the cached name list for injection into the sidebar prompt
- `GetUrl(string exactName, NhsIndexType)` → `string?` — returns the cached URL for an exact name (synchronous dictionary lookup; no async needed)

**Risk:** NHS page structure changes could break parsing → mitigation: if parsing returns zero entries the service logs a warning and the sidebar shows no entries; no exception is surfaced to the user.

### 4. Sidebar logic added to `ConversationService` as a new method

A new `GetRelatedConditionsAsync()` method is added to `IConversationService` and `ConversationService`. It calls `INhsIndexService.GetNames()` for both index types, builds the sidebar prompt (name lists + conversation history + selection instruction), sends it to `IOllamaClient`, deserialises the JSON response, then calls `INhsIndexService.GetUrl()` for each returned name to attach the NHS URL. It returns a `SidebarResult`. The UI invokes it after each successful assistant response.

### 5. Two-column layout using MudBlazor Grid

Once `_hasStarted` is true, the page switches from a single `MudContainer` to a `MudGrid` with a chat column (8/12) and a sidebar column (4/12). On small screens MudBlazor's grid collapses these to full-width stacked panels.

### 6. Disclaimer rendered at the top of the sidebar, always visible

The non-diagnostic disclaimer is a `MudAlert` with `Severity.Warning` pinned at the top of the sidebar panel. It is never dismissible. Its text explicitly states the suggestions are not a diagnosis and are for AI capability demonstration only.

## Risks / Trade-offs

- **NHS index page unavailable on first use** → `INhsIndexService` returns empty name lists; the sidebar prompt contains no NHS names so the LLM returns empty arrays; the sidebar shows nothing. The main conversation is unaffected.
- **NHS page structure changes break parsing** → Service logs a warning and returns empty caches; the sidebar shows nothing rather than crashing.
- **NHS name list adds context to each sidebar call** → Names only (no URLs or descriptions) keeps this to ~3–5 KB, acceptable for local Ollama models. Mitigated further by the call being non-blocking.
- **LLM returns a name not in the index** → `GetUrl()` returns null for that entry; it is silently dropped. The LLM is instructed to return only exact names from the list, so this should be rare.
- **JSON parse failure from LLM** → `GetRelatedConditionsAsync` returns an empty `SidebarResult`; the sidebar shows nothing rather than crashing.
- **Sidebar call increases Ollama load** → Each turn makes three LLM calls (main response, continuation check, sidebar). On slower hardware this may extend perceived latency. Mitigation: sidebar call is non-blocking.
- **Sidebar may display conditions that alarm the user** → The disclaimer and the medical-history-assistant framing mitigate this; the feature is explicitly positioned as an AI capability demonstration.

## Open Questions

- Should the sidebar accumulate conditions across turns (union of all turns) or only show the latest turn's result? (Proposed: accumulate, deduplicating by exact name.)

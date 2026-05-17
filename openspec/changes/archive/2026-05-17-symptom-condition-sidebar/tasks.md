## 1. Domain and Contract

- [x] 1.1 Add `RelatedCondition` record to the domain (`Name` string, `Url` string?)
- [x] 1.2 Add `SidebarResult` record to the domain (two `IReadOnlyList<RelatedCondition>` properties: `Conditions` and `Symptoms`)
- [x] 1.3 Add `GetRelatedConditionsAsync()` method to `IConversationService` returning `Task<SidebarResult>`

## 2. NHS Index Service

- [x] 2.1 Define `INhsIndexService` interface with `GetNamesAsync(NhsIndexType)` returning `Task<IReadOnlyList<string>>` and `GetUrl(string exactName, NhsIndexType)` returning `string?`
- [x] 2.2 Define `NhsIndexType` enum with values `Conditions` and `Symptoms`
- [x] 2.3 Implement `NhsIndexService`: on first call per index type, fetch the appropriate NHS index page using a named `HttpClient`, parse all `<a>` elements whose `href` begins with `/conditions/` or `/symptoms/`, build a case-insensitive `Dictionary<string, string>` of link text → absolute URL, and cache it
- [x] 2.4 Implement `GetNamesAsync`: ensure index is loaded, return cached keys as a list for prompt injection
- [x] 2.5 Implement `GetUrl`: synchronous exact lookup against the in-memory cache; return null if not found or cache not yet populated
- [x] 2.6 If fetching or parsing fails, log a warning, cache an empty dictionary for that index type, and return null; do not throw
- [x] 2.7 Register `NhsIndexService` as a singleton in `Program.cs` with a named `HttpClient` distinct from the Ollama client

## 3. Prompt and Conversation Service

- [x] 3.1 Add `BuildSidebarPrompt(IReadOnlyList<string> conditionNames, IReadOnlyList<string> symptomNames)` static method to `PromptTemplates` — injects NHS name lists into the prompt and instructs the LLM to return only exact names from those lists as JSON arrays
- [x] 3.2 Implement `GetRelatedConditionsAsync()` in `ConversationService`: call `GetNamesAsync` for both index types, build the sidebar prompt with injected names, send conversation history plus prompt to `IOllamaClient`, deserialise the JSON response (arrays of exact name strings), call `GetUrl` for each name, drop entries with no URL match, return the populated `SidebarResult`; return an empty `SidebarResult` on any parse failure

## 4. UI Layout

- [x] 4.1 Refactor `Home.razor` conversation view from a single `MudContainer` to a `MudGrid` with two columns: chat (8/12) and sidebar (4/12)
- [x] 4.2 Extract existing chat markup into the left grid column; sidebar panel occupies the right column

## 5. Sidebar Component

- [x] 5.1 Add sidebar state to `Home.razor`: `_sidebarConditions` and `_sidebarSymptoms` lists, `_sidebarLoading` bool
- [x] 5.2 Render the non-dismissible disclaimer `MudAlert` (Warning severity) as the first element of the sidebar panel
- [x] 5.3 Render a "Related Conditions" section: each entry rendered as an `<a>` link (`target="_blank"`, `rel="noopener noreferrer"`) to its resolved NHS URL
- [x] 5.4 Render a "Related Symptoms" section with the same link rendering
- [x] 5.5 Show a `MudProgressLinear` loading indicator inside the sidebar while `_sidebarLoading` is true
- [x] 5.6 Show a neutral placeholder message ("Conditions and symptoms will appear here as the conversation progresses.") when both lists are empty and not loading

## 6. Sidebar Call Integration

- [x] 6.1 After each successful assistant response in `StartConversation`, fire the sidebar analysis as a background call: set `_sidebarLoading = true`, `StateHasChanged()`, await `GetRelatedConditionsAsync()`, merge results into `_sidebarConditions` / `_sidebarSymptoms` (deduplicate by name), set `_sidebarLoading = false`, `StateHasChanged()`
- [x] 6.2 Apply the same background sidebar call in `SendReply` after each assistant response
- [x] 6.3 Ensure sidebar call exceptions are caught and suppressed without affecting the main conversation flow
- [x] 6.4 Do not run the sidebar call after the session has ended (`_sessionEnded`)

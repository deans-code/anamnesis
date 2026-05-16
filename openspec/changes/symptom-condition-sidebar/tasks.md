## 1. Domain and Contract

- [ ] 1.1 Add `RelatedCondition` record to the domain (`Name` string, `Synonyms` IReadOnlyList<string>, `Url` string? — null when no NHS match was found)
- [ ] 1.2 Add `SidebarResult` record to the domain (two `IReadOnlyList<RelatedCondition>` properties: `Conditions` and `Symptoms`)
- [ ] 1.3 Add `GetRelatedConditionsAsync()` method to `IConversationService` returning `Task<SidebarResult>`

## 2. NHS Index Service

- [ ] 2.1 Define `INhsIndexService` interface with a single method `ResolveUrlAsync(string name, IEnumerable<string> synonyms, NhsIndexType indexType)` returning `Task<string?>` (null when no match found)
- [ ] 2.2 Define `NhsIndexType` enum with values `Conditions` and `Symptoms`
- [ ] 2.3 Implement `NhsIndexService`: on first call per index type, fetch the appropriate NHS index page using a named `HttpClient`, parse all `<a>` elements whose `href` begins with `/conditions/` or `/symptoms/`, build a case-insensitive `Dictionary<string, string>` of link text → absolute URL, and cache it
- [ ] 2.4 Implement matching in `NhsIndexService.ResolveUrlAsync`: check the primary name then each synonym in order; return the first matched URL or null
- [ ] 2.5 If fetching or parsing fails, log a warning, cache an empty dictionary for that index type, and return null; do not throw
- [ ] 2.6 Register `NhsIndexService` as a singleton in `Program.cs` with a named `HttpClient` distinct from the Ollama client

## 3. Prompt and Conversation Service

- [ ] 3.1 Add `SidebarPrompt` constant to `PromptTemplates` — instructs the LLM to return a JSON object with `conditions` and `symptoms` arrays; each entry has `name` (primary term) and `synonyms` (array of alternate names, lay terms, and medical equivalents); no URLs
- [ ] 3.2 Implement `GetRelatedConditionsAsync()` in `ConversationService`: send conversation history plus sidebar prompt to `IOllamaClient`, deserialise the JSON response, call `INhsIndexService.ResolveUrlAsync` for each entry to obtain a verified NHS URL, return the populated `SidebarResult`; return an empty `SidebarResult` on any parse failure

## 4. UI Layout

- [ ] 4.1 Refactor `Home.razor` conversation view from a single `MudContainer` to a `MudGrid` with two columns: chat (8/12) and sidebar (4/12)
- [ ] 4.2 Extract existing chat markup into the left grid column; sidebar panel occupies the right column

## 5. Sidebar Component

- [ ] 5.1 Add sidebar state to `Home.razor`: `_sidebarConditions` and `_sidebarSymptoms` lists, `_sidebarLoading` bool
- [ ] 5.2 Render the non-dismissible disclaimer `MudAlert` (Warning severity) as the first element of the sidebar panel
- [ ] 5.3 Render a "Related Conditions" section: entries with a resolved URL as `<a>` links (`target="_blank"`, `rel="noopener noreferrer"`); entries without a URL as plain `MudText`
- [ ] 5.4 Render a "Related Symptoms" section with the same conditional link treatment
- [ ] 5.5 Show a `MudProgressLinear` loading indicator inside the sidebar while `_sidebarLoading` is true
- [ ] 5.6 Show a neutral placeholder message ("Conditions and symptoms will appear here as the conversation progresses.") when both lists are empty and not loading

## 6. Sidebar Call Integration

- [ ] 6.1 After each successful assistant response in `StartConversation`, fire the sidebar analysis as a background call: set `_sidebarLoading = true`, `StateHasChanged()`, await `GetRelatedConditionsAsync()`, merge results into `_sidebarConditions` / `_sidebarSymptoms` (deduplicate matched entries by URL, unmatched entries by primary name), set `_sidebarLoading = false`, `StateHasChanged()`
- [ ] 6.2 Apply the same background sidebar call in `SendReply` after each assistant response
- [ ] 6.3 Ensure sidebar call exceptions are caught and suppressed without affecting the main conversation flow
- [ ] 6.4 Do not run the sidebar call after the session has ended (`_sessionEnded`)

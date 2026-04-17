## 1. Project Scaffold

- [x] 1.1 Create a new Blazor Server project (`dotnet new blazorserver`) targeting .NET 10
- [x] 1.2 Add MudBlazor NuGet package and configure it in `Program.cs` and `_Imports.razor`
- [x] 1.3 Add MudBlazor CSS/JS references to `App.razor` / `_Host.cshtml`
- [ ] 1.4 Write README with setup instructions: install .NET 10 SDK, run Ollama locally, run the app with `dotnet run`
- [x] 1.5 Add `OllamaSettings` configuration class and populate defaults in `appsettings.json` (endpoint: `http://localhost:11434`, model: `medgemma`)

## 2. Ollama Backend Integration

- [x] 2.1 Create `OllamaClient` class registered as a typed `HttpClient` in DI (`builder.Services.AddHttpClient<OllamaClient>`)
- [x] 2.2 Implement `Task<string> ChatAsync(IEnumerable<OllamaMessage> messages)` that POSTs to `/api/chat` with `"stream": false` and returns the full response content
- [x] 2.3 Map Ollama connection errors (connection refused, timeout) to `OllamaUnavailableException`
- [x] 2.4 Create `PromptTemplates` static class with: (a) MedGemma system prompt (medical history assistant, one question at a time, no diagnoses), (b) summary instruction prompt, (c) continuation-check prompt (asks MedGemma to return `CONTINUE` or `END` based on conversation completeness)
- [x] 2.5 Create `ConversationService` (scoped) that maintains message history, prepends the system prompt, delegates to `OllamaClient`, and exposes `SendAsync()`, `CheckContinuationAsync()`, and `RequestSummaryAsync()`

## 3. UI Layout (MudBlazor)

- [x] 3.1 Replace the default MudBlazor layout (`MainLayout.razor`) with an app shell: `MudAppBar` (title + disclaimer badge), `MudMainContent`, `MudAppBar` footer with persistent disclaimer text
- [x] 3.2 Create `Pages/Index.razor` as the main page with a two-panel layout: chat history area and input area
- [x] 3.3 Style chat messages using `MudPaper` + CSS classes to distinguish user messages (right-aligned) from MedGemma messages (left-aligned)
- [x] 3.4 Add "End Session" `MudButton` (disabled by default, enabled after first LLM response)

## 4. Symptom Intake

- [x] 4.1 Render the symptom intake form (`MudTextField` + `MudButton`) when no conversation has started
- [x] 4.2 Validate that the input is non-empty/non-whitespace; show `MudAlert` on invalid submission
- [x] 4.3 On valid submission, hide the intake form, add the user message to the chat view, and invoke `ConversationService` to start the session

## 5. LLM Conversation Flow

- [x] 5.1 Wire the initial symptom submission to call `ConversationService.SendAsync()`, show animated loading indicator while awaiting response, then render the full MedGemma question in the chat on return
- [x] 5.2 Add a `MudTextField` + send button for subsequent replies (shown after intake form is dismissed)
- [x] 5.3 Disable the reply input, send button, and End Session button while an LLM call is in progress; re-enable on completion
- [x] 5.4 Append each completed LLM response to the chat history list
- [x] 5.5 After rendering the LLM question, call `ConversationService.CheckContinuationAsync()` in the background; if it returns `END`, automatically trigger session end and summary generation
- [x] 5.6 Add an animated loading indicator (e.g., a looping GIF or MudBlazor skeleton/spinner) displayed in the chat while any LLM call is processing

## 6. Session Summary

- [x] 6.1 Extract a `EndSessionAsync()` method in the component that: disables all inputs and the End Session button, calls `ConversationService.RequestSummaryAsync()`, shows the loading indicator, and renders the result
- [x] 6.2 Trigger `EndSessionAsync()` when the user clicks "End Session" OR when `CheckContinuationAsync()` returns `END`
- [x] 6.3 Render the summary in a distinct `MudAlert` (severity: Info) or highlighted `MudPaper` block below the chat
- [x] 6.4 Add a `MudIconButton` (clipboard icon) on the summary block; on click, call `JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", ...)` and briefly show a "Copied!" `MudSnackbar`

## 7. Error Handling

- [x] 7.1 Catch `OllamaUnavailableException` in the Blazor component and render a `MudAlert` with severity Error and instructions to run `ollama serve`
- [x] 7.2 Catch general LLM call errors and display a dismissible `MudAlert` without crashing the session

## 8. Polish & Validation

- [ ] 8.1 Test full conversation flow end-to-end with Ollama running MedGemma
- [ ] 8.2 Test automatic session end when LLM returns `END` from continuation check
- [ ] 8.3 Test manual "End Session" button triggers summary correctly
- [ ] 8.4 Test with Ollama stopped and confirm the `OllamaUnavailableException` error message is shown
- [ ] 8.5 Test empty input validation on the intake form
- [ ] 8.6 Test session summary generation and clipboard copy
- [ ] 8.7 Confirm the animated loading indicator appears during all LLM calls
- [ ] 8.8 Confirm the medical disclaimer is visible at all times (AppBar and footer)

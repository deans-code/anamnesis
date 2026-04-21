# Issue: ASI-01 — Prompt Injection

**Status:** [ ] Not Addressed  
**Risk Level:** CRITICAL  
**Source:** `code-analysis/owasp-compliance/2026-04-21-14-30.md`

---

## Description

User input is not validated or sanitized before being sent to the LLM. The system relies entirely on the system prompt's safety guidelines to prevent malicious input from causing harmful output.

## Evidence

- `ConversationService.SendAsync()` (line 23-35 of `ConversationService.cs`) adds user messages directly to the conversation history without any sanitization:
  ```csharp
  _history.Add(new ConversationMessage("user", userMessage));
  var response = await _ollamaClient.ChatAsync(_history);
  ```

- `Home.razor` accepts raw user input via `MudTextField` with no server-side validation beyond non-empty checks:
  ```csharp
  if (string.IsNullOrWhiteSpace(_symptomInput))
  {
      _showValidationError = true;
      return;
  }
  ```

- No input validation patterns found: no `validate_input`, `sanitize`, `classify_intent`, or `threat_detect` logic anywhere in the codebase.

- No negative patterns (eval, exec, subprocess) found — the application does not execute user input as code.

## What Passing Looks Like

```csharp
// GOOD: Validate before tool execution
var validated = inputValidator.Sanitize(userMessage);
if (validated.IsMalicious)
    return "Request blocked by policy";
var response = await _ollamaClient.ChatAsync(validated.CleanedInput);
```

## TODO: Steps to Fix

- [ ] Create an `IInputValidator` interface in `Anamnesis.Domain` or a new `Anamnesis.Security` project
- [ ] Implement input length validation (e.g., max 10,000 characters)
- [ ] Implement content classification to detect jailbreak attempts or malicious prompts
- [ ] Add output filtering to prevent the LLM from generating harmful content
- [ ] Implement a deterministic policy layer (e.g., keyword filtering) before LLM calls
- [ ] Inject `IInputValidator` into `ConversationService` and apply validation before each LLM call
- [ ] Add unit tests for the input validator covering edge cases (empty, null, oversized, malicious patterns)
- [ ] Add integration tests verifying that malicious input is blocked before reaching the LLM

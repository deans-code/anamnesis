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
# ASI-01: Prompt Injection — User Input Not Validated Before LLM Calls

## Status

- [ ] Addressed

## Risk

User-provided input flows directly into the LLM context without any validation, sanitization, or intent classification. This creates a prompt injection vulnerability where a malicious user could attempt to override the system prompt or extract sensitive information.

## Finding

**Status:** FAIL — No input validation before tool execution

### Evidence

**File:** `src/Anamnesis.UseCase.Conversation/ConversationService.cs`

```csharp
public async Task<string> SendAsync(string userMessage)
{
    if (!_systemPromptAdded)
    {
        _history.Insert(0, new ConversationMessage("system", PromptTemplates.MedGemmaSystemPrompt));
        _systemPromptAdded = true;
    }

    _history.Add(new ConversationMessage("user", userMessage));  // ← No validation

    var response = await _ollamaClient.ChatAsync(_history);
    _history.Add(new ConversationMessage("assistant", response));

    return response;
}
```

**File:** `src/Anamnesis.Interface.Website/Components/Pages/Home.razor`

```csharp
// Task 4.2: Validate non-empty input
if (string.IsNullOrWhiteSpace(_symptomInput))
{
    _showValidationError = true;
    return;
}
// Only checks for empty — no content validation
```

### Analysis

- User input is only checked for emptiness (`string.IsNullOrWhiteSpace`)
- No intent classification or content filtering is performed
- No prompt injection detection or sanitization
- The system prompt is prepended to the conversation history, making it vulnerable to context-based injection attacks
- A malicious user could craft input that attempts to override system instructions

### What Passing Looks Like

```csharp
// GOOD: Validate before tool execution
var validatedInput = await _inputValidator.ValidateAsync(userMessage);
if (validatedInput.Action == "deny")
{
    return "Request blocked by policy";
}
var response = await _ollamaClient.ChatAsync(validatedInput.Messages);
```

## Recommendation

1. Implement an input validation pipeline that runs before messages reach the LLM
2. Add intent classification to detect potential prompt injection attempts
3. Implement content filtering for medical-related inputs
4. Add output validation to detect if the LLM has been influenced to behave unexpectedly
5. Consider using a dedicated prompt injection detection library or service

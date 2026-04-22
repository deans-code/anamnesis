# ASI-05: Trust Boundary Violation — Blind Trust of LLM Responses

## Status

- [x] Addressed

## Risk

The agent blindly trusts LLM responses for control flow decisions without any verification, signature checking, or integrity validation. This creates a risk of trust boundary violations where manipulated LLM output could alter application behavior.

## Finding

**Status:** FAIL — No trust verification between components

### Evidence

**File:** `src/Anamnesis.UseCase.Conversation/ConversationService.cs`

```csharp
public async Task<bool> CheckContinuationAsync()
{
    var checkMessages = new List<ConversationMessage>(_history)
    {
        new ConversationMessage("user", PromptTemplates.ContinuationCheckPrompt)
    };

    var response = await _ollamaClient.ChatAsync(checkMessages);
    // ← Trusts LLM output for control flow without verification
    return !Regex.IsMatch(response.Trim(), @"\bEND\b", RegexOptions.IgnoreCase);
}
```

**File:** `src/Anamnesis.UseCase.Conversation/PromptTemplates.cs`

```csharp
public const string ContinuationCheckPrompt = """
    Review the conversation so far. Have you gathered enough information about the reported symptoms to produce a useful summary for a healthcare provider?

    You have enough information if you have explored most of: onset, duration, severity, character, location, associated symptoms, and relevant context.
    You do NOT need complete information on every dimension — a reasonable picture of the main symptoms is sufficient.

    Respond with ONLY one word:
    - CONTINUE if asking one more question would meaningfully improve the summary
    - END if you have enough for a useful summary
    """;
```

### Analysis

- `CheckContinuationAsync()` uses a fragile `Regex.IsMatch` to parse untrusted LLM output
- The LLM response is used directly for session control flow without any integrity check
- No signature verification on any inter-component communication
- No trust score checks before accepting delegated tasks
- No delegation narrowing (child scope <= parent scope)
- The system prompt relies entirely on the LLM following instructions — no deterministic enforcement

### What Passing Looks Like

```csharp
// GOOD: Trust verification between components
public async Task<bool> CheckContinuationAsync()
{
    var checkMessages = new List<ConversationMessage>(_history)
    {
        new ConversationMessage("user", PromptTemplates.ContinuationCheckPrompt)
    };

    var response = await _ollamaClient.ChatAsync(checkMessages);
    
    // Verify response integrity
    if (!VerifyResponseIntegrity(response))
        throw new SecurityError("Response integrity check failed");
    
    // Deterministic parsing, not LLM-dependent
    var parsed = ParseContinuationResponse(response);
    return parsed == ContinuationAction.Continue;
}

private bool VerifyResponseIntegrity(string response)
{
    // Check response matches expected format
    return response.Trim().Equals("END", StringComparison.OrdinalIgnoreCase)
        || response.Trim().Equals("CONTINUE", StringComparison.OrdinalIgnoreCase);
}
```

## Recommendation

1. Add response integrity verification before using LLM output for control flow
2. Implement deterministic parsing of LLM responses instead of regex-based parsing
3. Add trust verification between components
4. Implement response format validation with strict schemas
5. Consider adding signed responses for inter-component communication

- [ ] Addressed

# ASI-08: Policy Bypass — No Deterministic Policy Enforcement

## Risk

The only policy control is the system prompt text, which is entirely LLM-dependent. There is no deterministic policy enforcement layer, no allow/deny logic, and no fail-closed behavior. If the system prompt is altered or the model ignores it, there is no fallback.

## Finding

**Status:** FAIL — No deterministic policy enforcement

### Evidence

**File:** `src/Anamnesis.UseCase.Conversation/PromptTemplates.cs`

```csharp
public const string MedGemmaSystemPrompt = """
    You are a medical history assistant. Your role is to help users articulate and explore their symptoms through conversation.

    IMPORTANT GUIDELINES:
    - Ask exactly ONE focused follow-up question per response
    - Build on symptoms the user has already described
    - Probe systematically for: onset, duration, severity (1-10 scale), location, character, associated symptoms, relieving factors, aggravating factors
    - Keep questions clear, concise, and compassionate
    - Do NOT provide diagnoses, medical advice, or treatment recommendations
    - Do NOT speculate about conditions or suggest what the user might have
    - If the user asks for a diagnosis, politely decline and redirect to symptom exploration
    """;
```

**File:** `src/Anamnesis.UseCase.Conversation/ConversationService.cs`

```csharp
public async Task<string> SendAsync(string userMessage)
{
    if (!_systemPromptAdded)
    {
        _history.Insert(0, new ConversationMessage("system", PromptTemplates.MedGemmaSystemPrompt));
        _systemPromptAdded = true;
    }

    _history.Add(new ConversationMessage("user", userMessage));

    var response = await _ollamaClient.ChatAsync(_history);  // ← No policy check before tool call
    _history.Add(new ConversationMessage("assistant", response));

    return response;
}
```

### Analysis

- The only policy is the system prompt text — entirely LLM-dependent
- No deterministic guardrails, no allow/deny logic
- No fail-closed behavior — if policy check errors, action is allowed
- No policy evaluation layer separate from the LLM
- The system prompt can be overridden by a skilled prompt injection attack
- No deterministic policy enforcement in the code path
- No policy checks that cannot be skipped or overridden by the agent

### What Passing Looks Like

```csharp
// GOOD: Deterministic policy enforcement
public class PolicyEvaluator
{
    private readonly PolicyEngine _engine;
    
    public PolicyDecision Evaluate(string userInput, string context)
    {
        // Deterministic evaluation, no LLM involved
        var result = _engine.Evaluate(userInput, context);
        
        // Fail-closed: if evaluation fails, deny
        if (result == null)
            return PolicyDecision.Deny;
        
        return result.Action == "allow" 
            ? PolicyDecision.Allow 
            : PolicyDecision.Deny;
    }
}

public async Task<string> SendAsync(string userMessage)
{
    var policyDecision = _policyEvaluator.Evaluate(userMessage, context);
    if (policyDecision == PolicyDecision.Deny)
        throw new PolicyViolationException("Request blocked by policy");
    
    _history.Add(new ConversationMessage("user", userMessage));
    var response = await _ollamaClient.ChatAsync(_history);
    _history.Add(new ConversationMessage("assistant", response));
    
    return response;
}
```

## Recommendation

1. Implement a deterministic policy evaluation layer separate from the LLM
2. Add allow/deny logic for all tool calls
3. Implement fail-closed behavior — if policy check errors, action is denied
4. Add policy checks that cannot be skipped or overridden
5. Use YAML rules or code predicates for policy evaluation, not LLM calls
6. Ensure policy evaluation completes in <0.1ms without LLM involvement

# ASI-03: Excessive Agency — No Capability Boundaries

## Status

- [x] Addressed

## Risk

The agent has no defined capability boundaries or scope limits. It operates with unrestricted access to the Ollama chat API without any principle of least privilege enforcement.

## Finding

**Status:** FAIL — No capability boundaries defined

### Evidence

**File:** `src/Anamnesis.UseCase.Conversation/ConversationService.cs`

```csharp
public class ConversationService : IConversationService
{
    private readonly IOllamaClient _ollamaClient;
    private readonly List<ConversationMessage> _history = [];
    private bool _systemPromptAdded;

    public async Task<string> SendAsync(string userMessage)
    {
        // ...
        var response = await _ollamaClient.ChatAsync(_history);  // ← Unrestricted access
        // ...
    }

    public async Task<bool> CheckContinuationAsync()
    {
        // ...
        var response = await _ollamaClient.ChatAsync(checkMessages);  // ← Unrestricted access
        // ...
    }

    public async Task<string> RequestSummaryAsync()
    {
        // ...
        return await _ollamaClient.ChatAsync(summaryMessages);  // ← Unrestricted access
    }
}
```

### Analysis

- No capability list or execution ring limits what the agent can do
- The agent can make unlimited chat requests to the Ollama API
- No scope limits on what the agent can access
- No principle of least privilege applied to tool access
- The agent has full read/write access to conversation history without boundaries

### What Passing Looks Like

```csharp
// GOOD: Execution rings limit capabilities
public class AgentCapabilities
{
    public static readonly HashSet<string> AllowedTools = new() { "chat" };
    public static readonly int MaxMessagesPerSession = 50;
    public static readonly int MaxMessageLength = 4096;
}

public async Task<string> SendAsync(string userMessage)
{
    if (_history.Count >= AgentCapabilities.MaxMessagesPerSession)
        throw new AgencyLimitExceededException("Session message limit reached");
    
    if (userMessage.Length > AgentCapabilities.MaxMessageLength)
        throw new AgencyLimitExceededException("Message exceeds size limit");
    
    return await _ollamaClient.ChatAsync(_history);
}
```

## Recommendation

1. Define explicit capability boundaries for the agent
2. Implement scope limits on conversation history size and message length
3. Apply the principle of least privilege to tool access
4. Add session-level limits to prevent resource exhaustion
5. Consider implementing execution rings for different capability levels

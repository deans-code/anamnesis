# ASI-02: Insecure Tool Use — Unrestricted HTTP Tool Access

## Status

- [ ] Addressed

## Risk

The agent has unrestricted access to the Ollama API surface with no tool allowlist, argument validation, or parameter sanitization. This creates a risk of abuse through the exposed API endpoints.

## Finding

**Status:** FAIL — No tool allowlist or argument validation

### Evidence

**File:** `src/Anamnesis.Adapter.Ollama/OllamaClient.cs`

```csharp
public async Task<string> ChatAsync(IEnumerable<ConversationMessage> messages)
{
    var request = new OllamaChatRequestDto(
        Model: _settings.Model,  // ← Configurable, no validation
        Messages: messages.Select(m => new OllamaMessageDto(m.Role, m.Content)),
        Stream: false
    );

    var response = await _httpClient.PostAsJsonAsync("/api/chat", request);  // ← No endpoint allowlist
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadFromJsonAsync<OllamaChatResponseDto>();
    return result?.Message.Content ?? string.Empty;
}
```

**File:** `src/Anamnesis.Adapter.Ollama/OllamaSettings.cs`

```csharp
public class OllamaSettings
{
    public string BaseUrl { get; set; } = "http://localhost:11434";  // ← No validation
    public string Model { get; set; } = "medgemma";                  // ← No validation
}
```

### Analysis

- No tool allowlist restricts which Ollama API endpoints can be called
- The `Model` setting is configurable via appsettings with no validation — could potentially be manipulated
- No argument validation on the HTTP request parameters
- No rate limiting on tool calls
- The entire Ollama API surface is exposed through the `/api/chat` endpoint

### What Passing Looks Like

```csharp
// GOOD: Tool allowlist enforced
ALLOWED_TOOLS = {"chat"};

public async Task<string> ExecuteToolAsync(string toolName, Dictionary<string, object> args)
{
    if (!ALLOWED_TOOLS.Contains(toolName))
        throw new PermissionError($"Tool '{toolName}' not in allowlist");
    
    var validatedArgs = ValidateArgs(toolName, args);
    return await tools[toolName].ExecuteAsync(validatedArgs);
}
```

## Recommendation

1. Implement a tool allowlist restricting Ollama API calls to only the `/api/chat` endpoint
2. Add argument validation for all tool parameters
3. Pin the model name to a fixed value rather than allowing configuration
4. Implement rate limiting on tool calls
5. Add parameter size limits to prevent abuse

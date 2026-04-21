# Issue: ASI-02 — Insecure Tool Use

**Status:** [ ] Not Addressed  
**Risk Level:** HIGH  
**Source:** `code-analysis/owasp-compliance/2026-04-21-14-30.md`

---

## Description

The only tool in this system is the Ollama HTTP API. There is no tool allowlist, no argument validation, and user-controlled data flows directly into the API request body.

## Evidence

- `OllamaClient.ChatAsync()` (line 19-42 of `OllamaClient.cs`) constructs an HTTP POST with user messages directly in the request body:
  ```csharp
  var request = new OllamaChatRequestDto(
      Model: _settings.Model,
      Messages: messages.Select(m => new OllamaMessageDto(m.Role, m.Content)),
      Stream: false
  );
  var response = await _httpClient.PostAsJsonAsync("/api/chat", request);
  ```

- No tool allowlist mechanism exists anywhere in the codebase.

- No argument validation on the HTTP request — all message content is passed through as-is.

## What Passing Looks Like

```csharp
ALLOWED_TOOLS = {"search", "read_file", "create_ticket"}

def execute_tool(name: str, args: dict):
    if name not in ALLOWED_TOOLS:
        raise PermissionError(f"Tool '{name}' not in allowlist")
    # validate args...
    return tools[name](**validated_args)
```
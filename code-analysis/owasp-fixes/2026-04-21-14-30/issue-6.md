# Issue: ASI-06 — Insufficient Logging

**Status:** [ ] Not Addressed  
**Risk Level:** HIGH  
**Source:** `code-analysis/owasp-compliance/2026-04-21-14-30.md`

---

## Description

There is no structured audit trail for LLM tool calls. The application uses only standard ASP.NET Core logging with minimal configuration.

## Evidence

- `appsettings.json` configures logging at a high level only:
  ```json
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
  ```

- No structured logging for LLM API calls (no audit entries with timestamp, agent ID, tool name, args, result, policy decision).

- No tamper-evident logging (no hash-chained or append-only log format).

- Logs are not exported to secure storage.

## What Passing Looks Like

```csharp
// GOOD: Structured audit logging
_auditLogger.Log(new AuditEntry
{
    Timestamp = DateTime.UtcNow,
    AgentId = "anamnesis-agent",
    ToolName = "ollama-chat",
    InputLength = input.Length,
    OutputLength = output.Length,
    PolicyDecision = PolicyAction.Allow,
    SessionId = sessionId
});
```
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

## TODO: Steps to Fix

- [ ] Create an `IAuditLogger` interface in a new `Anamnesis.Security` project
- [ ] Implement structured JSONL audit logging for all LLM interactions
- [ ] Include: timestamp, session ID, input length, output length, policy decision
- [ ] Store logs in a separate, tamper-evident storage (e.g., Azure Blob Storage with immutability policy)
- [ ] Add log rotation and retention policies
- [ ] Add audit logging to `OllamaClient.ChatAsync()` for every LLM call
- [ ] Add audit logging to `ConversationService.SendAsync()` for every conversation action
- [ ] Add configuration for audit log path and retention in `appsettings.json`
- [ ] Add unit tests for the audit logger covering log format and rotation
- [ ] Add integration tests verifying that audit logs are written for all LLM interactions

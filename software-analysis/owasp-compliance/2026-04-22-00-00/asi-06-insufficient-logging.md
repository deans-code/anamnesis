# ASI-06: Insufficient Logging — No Audit Trail for Agent Actions

## Status

- [x] Addressed

## Risk

The agent produces no structured audit trail for its actions. There is no logging of LLM tool calls, user inputs, session events, or policy decisions. This creates compliance, forensic, and debugging challenges.

## Finding

**Status:** FAIL — No structured audit trail

### Evidence

**File:** `src/Anamnesis.Interface.Website/appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Analysis

- No structured logging of LLM tool calls (no request/response logging)
- No audit entries for user interactions, session events, or policy decisions
- ASP.NET logging only captures framework events at Warning level
- No append-only or hash-chained log format
- Logs are not stored separately from agent-writable directories
- No logging of conversation history or message content
- No logging of session start/end events
- No logging of error conditions or security events

### What Passing Looks Like

```csharp
// GOOD: Structured audit logging
public class AuditLogger
{
    private readonly IAppendOnlyLog _log;
    
    public async Task LogToolCallAsync(string agentId, string toolName, 
                                         object args, object result, 
                                         PolicyDecision policyDecision)
    {
        var entry = new AuditEntry
        {
            Timestamp = DateTime.UtcNow,
            AgentId = agentId,
            ToolName = toolName,
            Args = Serialize(args),
            Result = Serialize(result),
            PolicyDecision = policyDecision,
            ChainHash = ComputeChainHash()
        };
        
        await _log.AppendAsync(entry);
    }
}
```

## Recommendation

1. Implement structured audit logging for all LLM interactions
2. Log all user inputs, LLM outputs, and session events
3. Use append-only log format with chain hashes for tamper-evidence
4. Store logs separately from agent-writable directories
5. Include timestamp, agent ID, tool name, args, result, and policy decision in each entry
6. Export logs to secure storage for compliance purposes

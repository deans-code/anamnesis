# Issue: ASI-03 — Excessive Agency

**Status:** [ ] Not Addressed  
**Risk Level:** HIGH  
**Source:** `code-analysis/owasp-compliance/2026-04-21-14-30.md`

---

## Description

The agent has no explicit capability boundaries. The conversation history grows unbounded, and there are no scope limits on what the agent can do.

## Evidence

- `ConversationService._history` is an unbounded `List<ConversationMessage>` that grows indefinitely:
  ```csharp
  private readonly List<ConversationMessage> _history = [];
  ```

- No capability list or execution ring mechanism exists.

- No principle of least privilege applied to tool access.

## What Passing Looks Like

```csharp
// GOOD: Bounded conversation history
private const int MAX_HISTORY = 50;
private readonly Queue<ConversationMessage> _history = new();

public void AddMessage(ConversationMessage msg)
{
    _history.Enqueue(msg);
    while (_history.Count > MAX_HISTORY)
        _history.Dequeue();
}
```

## TODO: Steps to Fix

- [ ] Implement conversation history truncation (e.g., keep only last N messages, default 50)
- [ ] Define explicit capability boundaries (e.g., "this agent can only conduct symptom interviews")
- [ ] Add session timeout and maximum conversation length limits
- [ ] Implement a kill switch to terminate conversations
- [ ] Add a `ConversationHistoryTruncator` service that enforces the maximum history size
- [ ] Add configuration for max history size in `appsettings.json`
- [ ] Add unit tests for history truncation at the boundary
- [ ] Add integration tests verifying that oversized histories are truncated before LLM calls

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
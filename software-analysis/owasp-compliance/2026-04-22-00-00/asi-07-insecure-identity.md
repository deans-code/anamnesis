# ASI-07: Insecure Identity — No Cryptographic Agent Identity

## Status

- [ ] Addressed

## Risk

The agent has no cryptographic identity — it is identified only by configuration strings. There is no authentication between agents, no per-agent credentials, and no identity binding to capabilities.

## Finding

**Status:** FAIL — No cryptographic identity

### Evidence

**File:** `src/Anamnesis.Adapter.Ollama/OllamaSettings.cs`

```csharp
public class OllamaSettings
{
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "medgemma";  // ← String-only identification
}
```

**File:** `src/Anamnesis.Domain/ConversationMessage.cs`

```csharp
public record ConversationMessage(string Role, string Content);
// ← No agent identity, no cryptographic signing
```

**File:** `src/Anamnesis.Adapter.Ollama.Contract/IOllamaClient.cs`

```csharp
public interface IOllamaClient
{
    Task<string> ChatAsync(IEnumerable<ConversationMessage> messages);
    // ← No authentication or identity verification
}
```

### Analysis

- Agent is identified by `Model = "medgemma"` — a plain string with no cryptographic binding
- No authentication between components
- No per-agent credentials or rotation
- No DID-based identity (`did:web:`, `did:key:`)
- No cryptographic signing of agent messages or responses
- Shared configuration across all instances — no per-agent identity
- No identity bound to specific capabilities

### What Passing Looks Like

```csharp
// GOOD: Cryptographic agent identity
public class AgentIdentity
{
    public string Did { get; }           // did:key:z6Mk...
    public Ed25519PrivateKey PrivateKey { get; }
    public HashSet<string> Capabilities { get; }
    
    public string Sign(string payload)
    {
        return Crypto.Sign(payload, PrivateKey);
    }
    
    public static bool Verify(string payload, string signature, AgentIdentity identity)
    {
        return Crypto.Verify(payload, signature, identity.PublicKey);
    }
}
```

## Recommendation

1. Implement DID-based identity for the agent (`did:web:` or `did:key:`)
2. Add Ed25519 or similar cryptographic signing for agent messages
3. Implement per-agent credentials with rotation capability
4. Bind identity to specific capabilities
5. Add authentication between components

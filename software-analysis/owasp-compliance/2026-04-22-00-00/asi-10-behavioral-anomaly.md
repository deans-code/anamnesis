# ASI-10: Behavioral Anomaly — No Monitoring or Circuit Breakers

## Status

- [x] Addressed

## Risk

The system has no mechanism to detect, respond to, or stop abnormal agent behavior. There are no circuit breakers, no rate limiting, no drift detection, no kill switch, and no monitoring of tool call patterns.

## Finding

**Status:** FAIL — No drift detection, circuit breakers, or kill switch

### Evidence

**File:** `src/Anamnesis.Interface.Website/Program.cs`

```csharp
builder.Services.AddHttpClient<IOllamaClient, OllamaClient>((serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<OllamaSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(120);  // ← Only timeout, no circuit breaker
});
```

**File:** `src/Anamnesis.UseCase.Conversation/ConversationService.cs`

```csharp
public async Task<string> SendAsync(string userMessage)
{
    // ...
    var response = await _ollamaClient.ChatAsync(_history);  // ← No rate limiting
    // ...
}

public async Task<bool> CheckContinuationAsync()
{
    // ...
    var response = await _ollamaClient.ChatAsync(checkMessages);  // ← No rate limiting
    // ...
}
```

### Analysis

- No circuit breakers that trip on repeated failures
- No trust score decay over time (temporal decay)
- No kill switch or emergency stop capability
- No anomaly detection on tool call patterns (frequency, targets, timing)
- No rate limiting on LLM calls
- No monitoring of conversation patterns for abnormal behavior
- No mechanism to stop a misbehaving agent automatically
- No drift detection for model behavior changes

### What Passing Looks Like

```csharp
// GOOD: Behavioral monitoring with circuit breakers
public class AgentMonitor
{
    private readonly CircuitBreaker _breaker;
    private readonly RateLimiter _rateLimiter;
    private double _trustScore = 1.0;
    
    public async Task<T> ExecuteWithMonitoring<T>(Func<Task<T>> action, string context)
    {
        // Rate limiting
        if (!_rateLimiter.Allow())
            throw new RateLimitExceededException("Agent rate limit exceeded");
        
        // Circuit breaker
        if (_breaker.IsOpen())
            throw new CircuitBreakerOpenException("Circuit breaker tripped");
        
        try
        {
            var result = await action();
            _breaker.RecordSuccess();
            _trustScore = Math.Min(1.0, _trustScore + 0.01);
            return result;
        }
        catch (Exception ex)
        {
            _breaker.RecordFailure();
            _trustScore = Math.Max(0.0, _trustScore - 0.1);
            
            // Kill switch if trust drops too low
            if (_trustScore < 0.3)
                await TriggerKillSwitch();
            
            throw;
        }
    }
    
    private async Task TriggerKillSwitch()
    {
        // Emergency stop: disable agent, alert operators, preserve logs
        await _alertService.Notify("Agent kill switch triggered");
        await _logService.ExportLogs();
        _agentEnabled = false;
    }
}
```

## Recommendation

1. Implement circuit breakers that trip on repeated failures
2. Add trust score decay over time (temporal decay)
3. Implement a kill switch or emergency stop capability
4. Add anomaly detection on tool call patterns (frequency, targets, timing)
5. Implement rate limiting on LLM calls
6. Add monitoring for conversation patterns to detect abnormal behavior
7. Implement drift detection for model behavior changes

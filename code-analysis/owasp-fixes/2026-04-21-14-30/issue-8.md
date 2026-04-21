# Issue: ASI-08 — Policy Bypass

**Status:** [ ] Not Addressed  
**Risk Level:** CRITICAL  
**Source:** `code-analysis/owasp-compliance/2026-04-21-14-30.md`

---

## Description

Safety policy enforcement is entirely LLM-based (via the system prompt). There is no deterministic policy engine to prevent the agent from bypassing its constraints.

## Evidence

- `PromptTemplates.MedGemmaSystemPrompt` contains the safety guidelines:
  ```csharp
  public const string MedGemmaSystemPrompt = """
      ...
      IMPORTANT GUIDELINES:
      - Do NOT provide diagnoses, medical advice, or treatment recommendations
      - Do NOT speculate about conditions or suggest what the user might have
      - If the user asks for a diagnosis, politely decline and redirect to symptom exploration
      ...
      """;
  ```

- No deterministic policy engine exists. The system prompt is the only enforcement mechanism.
- No fail-closed behavior — if the LLM ignores the prompt, there is no fallback denial.

## What Passing Looks Like

```csharp
// GOOD: Deterministic policy enforcement
var policyResult = policyEvaluator.Evaluate(userInput);
if (policyResult.Action == PolicyAction.Deny)
    return "Request blocked by policy";
var response = await _ollamaClient.ChatAsync(userInput);
```

## TODO: Steps to Fix

- [ ] Create a `IPolicyEvaluator` interface in a new `Anamnesis.Security` project
- [ ] Implement a deterministic policy layer (e.g., keyword filtering, regex-based detection) before LLM calls
- [ ] Define safety rules as configuration (e.g., YAML or JSON rules file) rather than code
- [ ] Add output validation to detect and block harmful LLM responses
- [ ] Ensure policy checks cannot be bypassed or overridden by the agent
- [ ] Implement fail-closed behavior (if policy check errors, deny the action)
- [ ] Inject `IPolicyEvaluator` into `ConversationService` and apply before each LLM call
- [ ] Add unit tests for the policy evaluator covering allowed and denied inputs/outputs
- [ ] Add integration tests verifying that policy violations are blocked before reaching the LLM
- [ ] Add tests for edge cases (empty policy config, malformed rules, policy engine errors)

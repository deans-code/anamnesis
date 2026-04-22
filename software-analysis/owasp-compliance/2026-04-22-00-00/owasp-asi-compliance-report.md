# OWASP ASI Compliance Report

**Project:** Anamnesis - Medical Symptom Explorer  
**Generated:** 2026-04-22  
**Scope:** Full codebase review (src/ directory — 8 projects, 15 source files)  
**Agent Type:** AI agent with tool use (LLM API calls to Ollama/MedGemma)

## Summary: 2/10 Controls Covered

| Risk | Status | Finding |
|------|--------|---------|
| ASI-01 Prompt Injection | FAIL | No input validation before LLM calls; user symptoms sent directly to model |
| ASI-02 Insecure Tool Use | FAIL | No tool allowlist; raw HTTP POST to Ollama with unvalidated user input |
| ASI-03 Excessive Agency | FAIL | No capability boundaries; agent has unrestricted chat access |
| ASI-04 Unauthorized Escalation | PASS | No privilege escalation mechanisms present; single-role agent |
| ASI-05 Trust Boundary | FAIL | No identity verification between components; blind trust of LLM responses |
| ASI-06 Insufficient Logging | FAIL | No audit trail for tool calls; only default ASP.NET logging at Warning level |
| ASI-07 Insecure Identity | FAIL | No cryptographic identity; agent identified by configuration string only |
| ASI-08 Policy Bypass | FAIL | No deterministic policy enforcement; system prompt is the only control |
| ASI-09 Supply Chain | FAIL | No dependency integrity verification; no SBOM or signed packages |
| ASI-10 Behavioral Anomaly | FAIL | No drift detection, circuit breakers, or kill switch capability |

## Critical Gaps

### ASI-01: Prompt Injection — User input flows directly to LLM without sanitization
- `ConversationService.SendAsync()` adds user messages to history and sends to Ollama with no input validation
- No intent classification, no content filtering, no injection detection
- User can send arbitrary text that becomes part of the system prompt context

### ASI-02: Insecure Tool Use — Unrestricted HTTP tool access
- `OllamaClient.ChatAsync()` sends raw HTTP POST to `/api/chat` with no argument validation
- No allowlist of permitted operations; the entire Ollama API surface is exposed
- Model name is configurable via appsettings — user could potentially influence model selection

### ASI-05: Trust Boundary Violation — Blind trust of LLM responses
- `ConversationService.CheckContinuationAsync()` trusts LLM output for session control flow
- `Regex.IsMatch(response.Trim(), @"\bEND\b")` — fragile parsing of untrusted model output
- No signature verification or integrity check on any inter-component communication

### ASI-06: Insufficient Logging — No audit trail
- No structured logging of LLM tool calls (no request/response logging)
- No audit entries for user interactions, session events, or policy decisions
- ASP.NET logging only captures framework events at Warning level

### ASI-08: Policy Bypass — No deterministic policy enforcement
- The only policy is the system prompt text — entirely LLM-dependent
- No deterministic guardrails, no allow/deny logic, no fail-closed behavior
- If the system prompt is altered or the model ignores it, there is no fallback

### ASI-10: Behavioral Anomaly — No monitoring or circuit breakers
- No rate limiting on LLM calls
- No detection of abnormal conversation patterns
- No kill switch or emergency stop capability
- No trust score or drift detection

## Recommendations

1. **ASI-01**: Add input validation pipeline before messages reach the LLM. Implement intent classification and content filtering for medical-related inputs.
2. **ASI-02**: Implement a tool allowlist restricting Ollama API calls to only the `/api/chat` endpoint with validated parameters.
3. **ASI-05**: Add trust verification between components. Validate LLM responses before using them for control flow decisions.
4. **ASI-06**: Implement structured audit logging for all LLM interactions, user inputs, and session events. Use append-only log format.
5. **ASI-08**: Add deterministic policy enforcement layer. System prompt should be a supplement, not the sole control.
6. **ASI-10**: Implement circuit breakers, rate limiting, and behavioral monitoring for the agent.

## Risk Assessment

**Overall Risk Level: HIGH**

This is a medical-facing AI agent application that:
- Accepts unvalidated user input and sends it directly to an LLM
- Has no audit trail for compliance or forensic purposes
- Has no deterministic security controls — entirely dependent on LLM behavior
- Has no monitoring or anomaly detection
- Operates with no identity verification between components

While the application is a proof-of-concept running locally, these gaps would be critical in any production or healthcare-adjacent deployment.

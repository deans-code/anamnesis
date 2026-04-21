## Why

Users often struggle to fully articulate their medical symptoms or are unaware of related symptoms that might be clinically significant. This application provides a conversational, LLM-guided experience backed by a .NET Core server running entirely on local hardware (via Ollama + MedGemma) to help users explore and surface their symptoms more comprehensively — without sending sensitive health data to any external service or cloud provider.

## What Changes

- New Blazor Server application with a .NET Core backend and MudBlazor UI component library
- .NET Core backend proxies all requests to a locally-running Ollama instance (MedGemma model) — the browser has no direct contact with Ollama
- **No cloud-hosted LLM is used** — all inference is performed locally by Ollama; there is no OpenAI, Azure OpenAI, or any other cloud AI dependency
- Conversational symptom intake: MedGemma dynamically decides follow-up questions based on user input
- Symptom session summary generated at the end of the conversation

## Capabilities

### New Capabilities

- `symptom-intake`: Initial symptom description input — user describes what they are experiencing in free text
- `llm-conversation`: LLM-driven conversational engine that selects and presents follow-up questions to deepen symptom understanding
- `ollama-integration`: Server-side (.NET Core) integration with the local Ollama API (`/api/chat`) for MedGemma inference — **not a cloud LLM**
- `session-summary`: End-of-conversation summary of all reported symptoms and notable observations

### Modified Capabilities

<!-- No existing capabilities are being modified -->

## Impact

- New project: no existing code affected
- Requires .NET 10 SDK
- Depends on a locally-running Ollama instance (e.g., `http://localhost:11434`) with the MedGemma model pulled
- No external network requests for LLM inference — all data stays on-device; no cloud LLM integration
- Blazor Server frontend + ASP.NET Core backend in a single project; no database or persistent storage required

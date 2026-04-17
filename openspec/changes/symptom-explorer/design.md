## Context

This is a greenfield Blazor Server application with a .NET Core backend. There is no existing codebase. The .NET Core backend communicates with a locally-hosted Ollama instance running the **MedGemma** model. The browser never contacts Ollama directly — all LLM inference is proxied through the backend. No symptom data is sent to any cloud service or external network; everything runs on the user's local machine.

Users are expected to be running Ollama locally (e.g., `http://localhost:11434`) with the MedGemma model pulled (`ollama pull medgemma`).

## Goals / Non-Goals

**Goals:**
- Provide a clean, responsive conversational UI using MudBlazor components
- Backend (.NET Core) proxies all Ollama requests — the browser has no direct dependency on Ollama
- Use MedGemma (via local Ollama) for LLM inference; no cloud-hosted LLM is used
- Stream LLM responses token-by-token to the UI via Blazor Server's SignalR connection
- Use the LLM to dynamically determine which follow-up questions to ask
- Generate a readable session summary at the end of the conversation

**Non-Goals:**
- Medical diagnosis — the app explicitly disclaims any diagnostic intent
- Integration with any cloud-hosted LLM (OpenAI, Azure OpenAI, Gemini API, etc.) — **this app is local-only**
- Storing or transmitting symptom data externally
- User authentication or session persistence across application restarts
- Mobile-native apps
- Multi-tenant or multi-user deployment

## Decisions

### 1. Frontend: Blazor Server with MudBlazor

**Decision**: Use Blazor Server with MudBlazor as the component library.

**Alternatives considered**:
- Blazor WASM (hosted): Would require a separate API project for the Ollama proxy; adds deployment complexity for a local tool
- Blazor WASM (standalone): Cannot call Ollama directly without CORS configuration; would need a backend anyway
- Plain HTML/CSS/JS: No type safety, no component reuse, no .NET ecosystem

**Rationale**: Blazor Server keeps the entire stack in C#/.NET. The persistent SignalR connection between browser and server makes streaming token updates to the UI straightforward via `StateHasChanged()`. MudBlazor provides a polished Material Design component set with chat-friendly layout primitives (`MudPaper`, `MudTextField`, `MudButton`, etc.).

---

### 2. Backend: ASP.NET Core (.NET 10) with HttpClient calling local Ollama

**Decision**: The ASP.NET Core backend owns all communication with Ollama. A typed `HttpClient` (`OllamaClient`) is registered in DI and calls `POST http://localhost:11434/api/chat` with `"stream": false`. The full response is returned as a single `Task<string>` and forwarded to the Blazor component when complete.

**Alternatives considered**:
- Streaming (`"stream": true`): More complex response parsing; per-token UI updates add state complexity without meaningful UX benefit given the loading indicator approach
- Browser calling Ollama directly: Requires configuring Ollama's CORS headers and exposes the Ollama port dependency to the browser layer

**Rationale**: Non-streaming simplifies the backend and UI considerably. While the user waits, an animated loading indicator is shown. The full response is rendered at once when it arrives.

---

### 3. LLM Model: MedGemma via Ollama (local only)

**Decision**: The application uses the **MedGemma** model served by the local Ollama instance. MedGemma is a medically-focused open model well-suited for clinical history taking. The model name is fixed in application configuration (`appsettings.json`) and is not dynamically selectable at runtime.

**Alternatives considered**:
- General-purpose models (llama3, mistral): Less medically-grounded; more likely to stray from clinical context
- Cloud-hosted LLMs (OpenAI GPT-4, Google Gemini API, Azure OpenAI): **Explicitly excluded** — symptom data must not leave the local machine

**Rationale**: MedGemma is purpose-built for medical question answering and history-taking tasks, making it the best fit for this application's goals.

---

### 4. System Prompt Strategy: Role-focused prompt with structured output guidance

**Decision**: The system prompt instructs MedGemma to act as a **medical history assistant** (not a diagnostician). It must:
- Ask one focused question at a time
- Build on previously reported symptoms
- Probe for onset, duration, severity, associated symptoms, and relieving/aggravating factors
- When enough information has been gathered (or the user indicates they are done), produce a structured symptom summary

The system prompt is defined as a constant in a dedicated `PromptTemplates` class in the backend.

**Alternatives considered**:
- Letting the LLM free-form respond: Less predictable UX, harder to control question cadence
- Pre-scripted question trees: Rigid, misses context-specific follow-ups

**Rationale**: A well-crafted system prompt with conversational constraints gives MedGemma enough freedom to be useful while keeping the UX coherent.

---

### 5. LLM-Driven Conversation Termination

**Decision**: After each user reply, the backend makes a second LLM call (a lightweight continuation-check prompt) asking MedGemma whether sufficient information has been gathered to end the conversation or whether more questions are needed. MedGemma returns a structured signal (`CONTINUE` or `END`). If `END` is returned, the system automatically ends the session and generates the summary without further user action.

The user may also manually end the session at any time via the "End Session" button.

**Alternatives considered**:
- Fixed question count: Rigid; different symptoms require different depth
- Parsing the question response itself for signals: Fragile; conflates two concerns in one response

**Rationale**: A dedicated lightweight check keeps the main question response clean and allows the system to gracefully conclude when the LLM has enough context.

---

### 6. Session Summary: LLM-generated on session end (manual or automatic)

**Decision**: When the session ends (either via user clicking "End Session" or LLM signalling `END`), the backend appends a final instruction message to the conversation history and sends it to Ollama. MedGemma produces a structured symptom summary. The full summary is rendered at once in a visually distinct `MudAlert` or `MudPaper` block.

**Alternatives considered**:
- Parse conversation history server-side with regex/NLP: Too fragile
- Always append a running summary: Adds noise to the chat UI

**Rationale**: MedGemma already has full conversation context. A single structured prompt reliably produces a clean summary.

## Risks / Trade-offs

- **Ollama not running** → The `OllamaClient` catches `HttpRequestException` and surfaces a `MudAlert` with setup instructions (e.g., `ollama serve`)
- **Long conversations** → MedGemma's context window may be exceeded in very long sessions. Mitigation: the backend tracks approximate token count and trims the oldest non-system messages when the limit is approached (future enhancement)
- **Continuation-check latency** → Each turn requires two sequential LLM calls (question + continuation check). This doubles per-turn latency. Mitigation: the continuation check uses a minimal prompt to keep it fast; the loading indicator manages user expectation.
- **Input sanitization** → All LLM calls are server-side. User input is passed as message content, not interpolated into prompts as raw strings — this prevents prompt injection at the template level. The system prompt also instructs MedGemma not to act outside its medical history assistant role.
- **Blazor Server scalability** → This is a single-user local tool; Blazor Server's per-connection memory overhead is not a concern
- **No cloud fallback by design** → The app will not fall back to any cloud LLM if Ollama is unavailable. This is intentional — local-only operation is a hard requirement.

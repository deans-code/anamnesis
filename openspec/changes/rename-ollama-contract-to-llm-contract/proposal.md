## Why

`Anamnesis.Adapter.Ollama.Contract` exposes Ollama by name in its contract, leaking a technology choice into the boundary layer. The contract should declare that the system uses an LLM, not which one — exactly as `Anamnesis.Adapter.MedicalData.Contract` declares that the system uses a medical data source without naming NHS.

## What Changes

- **NEW** `Anamnesis.Adapter.Llm.Contract` project — technology-agnostic LLM adapter contract containing `LlmUnavailableException` and `RateLimitExceededException`; replaces `Anamnesis.Adapter.Ollama.Contract`
- **REMOVED** `Anamnesis.Adapter.Ollama.Contract` — replaced by `Anamnesis.Adapter.Llm.Contract`
- **MOVED** `IOllamaClient` — removed from the contract project; becomes an internal interface inside `Anamnesis.Adapter.Ollama`, with `InternalsVisibleTo` granting access to the test project
- **RENAMED** `OllamaUnavailableException` → `LlmUnavailableException` in namespace `Anamnesis.Adapter.Llm.Contract`; `RateLimitExceededException` moves to the same namespace unchanged
- **MODIFIED** `Anamnesis.Adapter.Ollama` — replaces contract project reference; `IOllamaClient` becomes internal; `using` directives and exception references updated throughout
- **MODIFIED** `Anamnesis.Adapter.Ollama.Test` — replaces contract project reference; adds `InternalsVisibleTo`-gated access to `IOllamaClient`; exception names updated
- **MODIFIED** `Anamnesis.Infrastructure` — replaces `Adapter.Ollama.Contract` reference with `Adapter.Llm.Contract`; `IOllamaClient` accessed from `Anamnesis.Adapter.Ollama` namespace

## Capabilities

### New Capabilities

- `llm-adapter-contract`: A standalone, technology-agnostic contract project defining the exception surface for any LLM adapter (`LlmUnavailableException`, `RateLimitExceededException`); `IOllamaClient` is an internal seam within the Ollama adapter and is not part of the contract

### Modified Capabilities

- `ollama-integration`: The Ollama adapter's internal structure changes — `IOllamaClient` becomes internal to `Adapter.Ollama`; the exceptions it throws are renamed from `OllamaUnavailableException` to `LlmUnavailableException` in the generic contract namespace

## Impact

- `Anamnesis.Adapter.Ollama.Contract` is deleted and replaced by `Anamnesis.Adapter.Llm.Contract`
- All `using Anamnesis.Adapter.Ollama.Contract;` statements replaced with `using Anamnesis.Adapter.Llm.Contract;` (and `using Anamnesis.Adapter.Ollama;` where `IOllamaClient` is needed)
- `OllamaUnavailableException` renamed to `LlmUnavailableException` in all call sites: `OllamaClient`, `OllamaConversationEngine`, `OllamaClientTests`
- `Anamnesis.Adapter.Ollama.csproj` gains `InternalsVisibleTo` for `Anamnesis.Adapter.Ollama.Test`
- No runtime behaviour changes

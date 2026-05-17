## Why

The web interface project currently references adapter projects directly, violating the Ports & Adapters rule that the interface layer should only depend on use case contracts. Additionally, `IMedicalReferenceService` is defined in the use case contract rather than in a technology-agnostic adapter contract, meaning the use case layer owns an interface that describes infrastructure rather than a use case need.

## What Changes

- **NEW** `Anamnesis.Adapter.MedicalData.Contract` project — a technology-agnostic adapter contract containing `IMedicalReferenceService`, replacing the empty `Anamnesis.Adapter.Nhs.Contract`
- **NEW** `Anamnesis.Infrastructure` project — the composition root responsible for wiring all adapter implementations into the DI container, exposing a single `AddAnamnesis(IServiceCollection, IConfiguration)` extension
- **REMOVED** `Anamnesis.Adapter.Nhs.Contract` — replaced by `Anamnesis.Adapter.MedicalData.Contract`
- **MODIFIED** `Anamnesis.UseCase.Conversation.Contract` — `IMedicalReferenceService` removed; use case contract no longer owns infrastructure-facing interfaces
- **MODIFIED** `Anamnesis.UseCase.Conversation` — gains direct reference to `Adapter.MedicalData.Contract`; `IMedicalReferenceService` used via the adapter contract
- **MODIFIED** `Anamnesis.Adapter.Nhs` — references `Adapter.MedicalData.Contract` instead of `UseCase.Conversation.Contract`; no longer depends on the use case layer
- **MODIFIED** `Anamnesis.Interface.Website` — removes all direct adapter project references; depends only on use case contracts, domain, and `Anamnesis.Infrastructure`

## Capabilities

### New Capabilities

- `medical-data-adapter-contract`: A standalone, technology-agnostic contract project defining what any medical reference data adapter must provide (`IMedicalReferenceService`), independent of both use case and NHS specifics
- `infrastructure-composition-root`: A dedicated composition root project (`Anamnesis.Infrastructure`) that owns all adapter wiring, keeping the web interface free of adapter knowledge

### Modified Capabilities

- `condition-sidebar`: The port through which medical reference data reaches the conversation use case is now defined in `Adapter.MedicalData.Contract` rather than `UseCase.Conversation.Contract`; the requirement for conditions and symptoms data is unchanged but the ownership of the interface definition moves

## Impact

- `Anamnesis.Adapter.Nhs.Contract` is deleted and replaced by `Anamnesis.Adapter.MedicalData.Contract`
- `Anamnesis.Adapter.Nhs.Test` project references updated from `Adapter.Nhs.Contract` to `Adapter.MedicalData.Contract`
- `Anamnesis.Interface.Website.csproj` loses references to `Adapter.Nhs`, `Adapter.Ollama`, `Adapter.Ollama.Contract`; gains reference to `Anamnesis.Infrastructure`
- All HTTP client configuration, rate limiter setup, DI registrations, and settings validation move from `Program.cs` into `Anamnesis.Infrastructure`
- No runtime behaviour changes — all adapter implementations, interfaces, and DI registrations remain functionally equivalent

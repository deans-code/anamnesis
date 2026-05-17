## Context

`INhsIndexService` and `NhsIndexType` currently live in `Anamnesis.UseCase.Conversation.Contract`, meaning every consumer of the use case contract is exposed to NHS-specific vocabulary. `ConversationService` calls `_nhsIndexService.GetNamesAsync(NhsIndexType.Conditions)` and uses `NhsIndexType.Symptoms` — two references that betray the data source. The implementation (`NhsIndexService`) sits in `Anamnesis.Interface.Website`, which is also wrong: website infrastructure should not own data-fetching adapters.

This mirrors the problem solved in the prior change (encapsulate-llm-in-adapter): an adapter-level concern has leaked into the use case layer.

## Goals / Non-Goals

**Goals:**
- Define `IMedicalReferenceService` in the use case contract as a technology-agnostic port returning `IReadOnlyList<RelatedCondition>` for conditions and symptoms.
- Create a new `Anamnesis.Adapter.Nhs` project that owns all NHS-specific infrastructure, implementing `IMedicalReferenceService` as `NhsConditionReferenceService`.
- Delete `INhsIndexService`, `NhsIndexType`, and `NhsIndexService` entirely.
- `ConversationService` uses `IMedicalReferenceService` with no reference to NHS, index types, or URL resolution logic.

**Non-Goals:**
- Changing the sidebar feature behaviour or the HTML parsing logic.
- Supporting multiple simultaneous `IMedicalReferenceService` implementations (e.g., SNOMED); that is a future concern.
- Altering the caching or resilience strategy of the existing implementation.

## Decisions

### D1: `IMedicalReferenceService` exposes two separate methods rather than a single parameterised one

**Decision**: `GetConditionsAsync()` and `GetSymptomsAsync()` as distinct methods, each returning `Task<IReadOnlyList<RelatedCondition>>`.

**Rationale**: The old `GetNamesAsync(NhsIndexType)` pattern leaked the idea of a shared index keyed by type, which is an implementation detail. Separate methods express the domain intent clearly — the use case needs conditions, and it needs symptoms — without implying anything about how they are stored or fetched. The return type `RelatedCondition` already carries the URL, so the use case no longer needs a separate `GetUrl` call.

**Alternative considered**: A single `GetAllAsync()` returning both together (e.g., a value type). Rejected because it forces both lists to be fetched together even if only one is needed, and it introduces a new domain type for a minor convenience.

### D2: New `Anamnesis.Adapter.Nhs` project rather than keeping the implementation in `Interface.Website`

**Decision**: Create a dedicated adapter project for the NHS data source.

**Rationale**: `Anamnesis.Interface.Website` is the composition root / UI host — it should wire dependencies, not own business logic or I/O. A dedicated adapter project makes the boundary explicit, mirrors the `Anamnesis.Adapter.Ollama` pattern, and allows the NHS adapter to be tested independently.

**Alternative considered**: Keep `NhsConditionReferenceService` in `Interface.Website`. Rejected because it perpetuates the wrong layering and is inconsistent with the Ollama adapter structure.

### D3: `NhsConditionReferenceService` retains the existing singleton caching strategy

**Decision**: `NhsConditionReferenceService` is registered as `Singleton` (matching the prior `NhsIndexService` registration), maintaining the in-memory cache and `SemaphoreSlim` fetch lock.

**Rationale**: The existing caching logic is correct and performant. This change is about moving code, not rewriting it.

### D4: `NhsIndexType` is deleted — no equivalent in the new adapter's public surface

**Decision**: `NhsIndexType` becomes an internal implementation detail (or is removed entirely) inside `NhsConditionReferenceService`.

**Rationale**: The enum existed only to parameterise the old shared-index interface. With separate `GetConditionsAsync`/`GetSymptomsAsync` methods, there is no need to expose the concept of an "index type" to any consumer.

## Risks / Trade-offs

- **New project requires solution file update** → `anamnesis.sln` must have `Anamnesis.Adapter.Nhs` added. Mitigation: use `dotnet sln add` as part of the task sequence.
- **`Interface.Website` reference graph changes** → Website now references `Anamnesis.Adapter.Nhs` instead of owning the implementation. Mitigation: straightforward `.csproj` edit; no transitive issues expected.
- **`ConversationServiceTests` mock surface changes** → Tests currently mock `INhsIndexService.GetNamesAsync` and `GetUrl`; they will need updating to mock `IMedicalReferenceService.GetConditionsAsync` and `GetSymptomsAsync`. Mitigation: the new mock is simpler (returns `IReadOnlyList<RelatedCondition>` directly).

## Migration Plan

1. Add `IMedicalReferenceService` to `Anamnesis.UseCase.Conversation.Contract`.
2. Create `Anamnesis.Adapter.Nhs` project and add to solution.
3. Create `NhsConditionReferenceService` implementing `IMedicalReferenceService`; move parsing logic from `NhsIndexService`.
4. Update `ConversationService` to depend on `IMedicalReferenceService`.
5. Delete `INhsIndexService.cs`, `NhsIndexType.cs` from contract; delete `NhsIndexService.cs` from website.
6. Update `Program.cs` DI registration.
7. Update `ConversationServiceTests`.
8. Build and run full test suite.

No database migrations, feature flags, or deployment coordination required. Rollback is a git revert.

## Open Questions

- None.

## Context

The Ollama adapter follows a three-project pattern: `Anamnesis.Adapter.Ollama.Contract` (public surface / exception types), `Anamnesis.Adapter.Ollama` (implementation), and `Anamnesis.Adapter.Ollama.Test` (unit tests). The NHS adapter was created with only the middle project. This change adds the two missing projects to make the pattern uniform.

## Goals / Non-Goals

**Goals:**
- Add `Anamnesis.Adapter.Nhs.Contract` as a consistently-structured placeholder, ready to hold exception types or internal interfaces if the adapter grows.
- Add `Anamnesis.Adapter.Nhs.Test` with tests covering `NhsConditionReferenceService`: successful HTML parsing, in-memory caching, and HTTP failure handling.

**Non-Goals:**
- Adding any exception types or interfaces to the contract project now — it starts empty of code files.
- Changing `NhsConditionReferenceService` behaviour.
- Adding logging assertions (would require a log-sink dependency; overkill for this change).

## Decisions

### D1: Contract project ships with no code files

**Decision**: `Anamnesis.Adapter.Nhs.Contract` contains only a `.csproj`. No interfaces or exception types are added now.

**Rationale**: There is currently no internal seam in the NHS adapter (unlike Ollama's `IOllamaClient`) and no exceptions that surface to callers. Forcing placeholder types would add noise with no value. The project exists as the designated landing zone when those needs arise.

### D2: Tests use MockHttpMessageHandler defined locally in the test project

**Decision**: `Anamnesis.Adapter.Nhs.Test` defines its own `MockHttpMessageHandler` (same pattern as Ollama). No shared test utilities project is introduced.

**Rationale**: The handlers are trivial (< 10 lines each). A shared project adds project-graph complexity for minimal gain at this scale. If a third adapter appeared, the trade-off would shift.

### D3: No InternalsVisibleTo required

**Decision**: `NhsConditionReferenceService` is `public`; `ParseIndex` is `private static` and tested indirectly via the public interface through HTTP mocking. No `InternalsVisibleTo` is needed.

**Rationale**: Testing through the public contract (mock HTTP → call `GetConditionsAsync`) is cleaner than exposing internals. The parsing logic is fully exercised via the input HTML and the returned `RelatedCondition` list.

### D4: Nhs.Contract is referenced by Nhs but not by the website

**Decision**: `Anamnesis.Adapter.Nhs` references `Anamnesis.Adapter.Nhs.Contract`; `Anamnesis.Interface.Website` does not add a new reference to it.

**Rationale**: Nothing in the contract needs to be visible to the composition root right now. If exception types were added later, the website reference would be added at that point.

## Migration Plan

1. Create `Anamnesis.Adapter.Nhs.Contract` project and add to solution.
2. Add reference from `Anamnesis.Adapter.Nhs` to `Anamnesis.Adapter.Nhs.Contract`.
3. Create `Anamnesis.Adapter.Nhs.Test` project and add to solution.
4. Write `NhsConditionReferenceServiceTests`.
5. Build and run full test suite.

## Open Questions

- None.

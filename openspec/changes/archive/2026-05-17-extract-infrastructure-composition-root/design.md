## Context

The solution currently has two structural violations of Ports & Adapters architecture:

1. `IMedicalReferenceService` is defined in `Anamnesis.UseCase.Conversation.Contract`. The use case contract owns an interface that describes infrastructure capability (fetching medical reference data), rather than a use case need. This also forces `Anamnesis.Adapter.Nhs` to depend on the use case contract in order to implement the interface — an adapter depending upward on the use case layer.

2. `Anamnesis.Interface.Website` references adapter projects directly (`Adapter.Nhs`, `Adapter.Ollama`, `Adapter.Ollama.Contract`). Blazor components and `Program.cs` are in the same project, so the interface layer transitively knows about infrastructure implementations that it should be shielded from.

The `Anamnesis.Adapter.Nhs.Contract` project currently exists but is empty — a structural placeholder with no content.

## Goals / Non-Goals

**Goals:**
- Move `IMedicalReferenceService` to a technology-agnostic adapter contract project (`Anamnesis.Adapter.MedicalData.Contract`), replacing the empty `Anamnesis.Adapter.Nhs.Contract`
- Create `Anamnesis.Infrastructure` as a dedicated composition root that owns all adapter wiring and DI registration
- Reduce `Anamnesis.Interface.Website`'s project references to use case contracts, domain, and `Anamnesis.Infrastructure` only
- Ensure `Anamnesis.Adapter.Nhs` no longer depends on the use case layer

**Non-Goals:**
- Changing any runtime behaviour, adapter implementations, or interface signatures
- Extracting `Program.cs` into a separate executable; `Interface.Website` remains the entry point
- Splitting the composition root across multiple projects

## Decisions

### 1. Replace `Adapter.Nhs.Contract` with `Adapter.MedicalData.Contract`

`Adapter.Nhs.Contract` is empty and was a structural placeholder. Rather than adding a second contract project alongside it, it is replaced outright. The new project name (`MedicalData`) describes the capability (medical reference data) without naming the technology (NHS). A future adapter — `Adapter.SNOMED`, `Adapter.OpenMRS` — would implement the same `IMedicalReferenceService` interface.

Alternative considered: keep `Adapter.Nhs.Contract` and add `Adapter.MedicalData.Contract` as a sibling. Rejected — maintaining two contract projects for one interface adds noise with no benefit at this scale.

### 2. `UseCase.Conversation` takes the direct reference to `Adapter.MedicalData.Contract`

`UseCase.Conversation.Contract` (the pure port definition) does not reference the adapter contract. The implementation project (`UseCase.Conversation`) takes the direct dependency. This keeps the contract project free of adapter-layer knowledge and avoids a downward reference from the use case contract into the adapter namespace.

Alternative considered: have `UseCase.Conversation.Contract` re-export `IMedicalReferenceService` transitively. Rejected — creates an implicit coupling and breaks the principle that adapters should not call each other through the use case contract.

### 3. `Anamnesis.Infrastructure` as the composition root

A dedicated project collects all adapter wiring: HTTP client configuration, rate limiter setup, settings validation, and DI registration for `IConversationEngine`, `IMedicalReferenceService`, `IAuditLogger`, and `IConversationService`. It exposes a single `AddAnamnesis(IServiceCollection, IConfiguration)` extension method.

`Interface.Website/Program.cs` becomes:
```csharp
builder.Services.AddAnamnesis(builder.Configuration);
```

The website project retains references only to `UseCase.Conversation.Contract` (for injecting `IConversationService` in components), `Domain` (for shared types in components), and `Anamnesis.Infrastructure` (for the registration call).

Alternative considered: adapter self-registration via extension methods called from `Program.cs`. Rejected — `Interface.Website` would still reference adapter projects to call those methods, which does not satisfy the constraint.

### 4. `Anamnesis.Infrastructure` project references

`Infrastructure` references all layers it wires:
- `UseCase.Conversation` + `UseCase.Conversation.Contract`
- `Adapter.MedicalData.Contract`
- `Adapter.Nhs`
- `Adapter.Ollama` + `Adapter.Ollama.Contract`
- `Domain`

This is intentional — the composition root is the one place where all layers meet.

## Risks / Trade-offs

- [Circular risk] `Infrastructure` must not be referenced by any adapter or use case project, only by the interface layer. There is no enforced mechanism preventing this at compile time without ArchUnit rules. → Mitigation: ArchUnit rules are a planned scope item in the README; this change makes that work more valuable.

- [Transitive type visibility] .NET SDK-style projects make transitively referenced types available at compile time even without an explicit project reference. `Interface.Website` components could still accidentally use adapter types via the `Infrastructure` transitive closure. → Mitigation: the explicit project reference list in the `.csproj` documents intent; ArchUnit enforces it at test time.

- [Single composition root] All adapter wiring lives in one project. If the solution grows to multiple interfaces (REST API, CLI), each would reference `Anamnesis.Infrastructure` and get all adapters even if only a subset is needed. → Acceptable at current scale; can be split later if needed.

## Migration Plan

1. Create `Adapter.MedicalData.Contract`, add `IMedicalReferenceService`, add to solution
2. Update `Adapter.Nhs` to reference `Adapter.MedicalData.Contract` instead of `UseCase.Conversation.Contract`
3. Update `Adapter.Nhs.Test` to reference `Adapter.MedicalData.Contract` instead of `Adapter.Nhs.Contract`
4. Remove `IMedicalReferenceService` from `UseCase.Conversation.Contract`; add reference to `Adapter.MedicalData.Contract` in `UseCase.Conversation`
5. Delete `Adapter.Nhs.Contract` and remove from solution
6. Create `Anamnesis.Infrastructure`, move all adapter wiring from `Program.cs`
7. Update `Interface.Website` references; `Program.cs` calls `AddAnamnesis()`
8. Build and test at each step; no intermediate state should break compilation

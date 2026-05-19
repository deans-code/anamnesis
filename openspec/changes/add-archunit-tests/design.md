## Context

The solution follows a Ports & Adapters (Hexagonal) architecture with clean layer separation:

- **Domain** (`Anamnesis.Domain`) — pure business entities; no dependencies on other layers
- **UseCase** (`Anamnesis.UseCase.<Name>`) — application logic; depends on Domain and its own Contract
- **UseCase Contract** (`Anamnesis.UseCase.<Name>.Contract`) — port interfaces; depends on Domain only
- **Adapter Contract** (`Anamnesis.Adapter.<DomainConcept>.Contract`) — driven-port interfaces named after the domain concept they model (e.g. `Llm`, `MedicalData`); depends on Domain only
- **Adapter** (`Anamnesis.Adapter.<DomainConcept>.<Technology>`) — driven-port implementations named after the domain concept and technology (e.g. `MedicalData.Nhs`, `Llm.Ollama`); depends on its matching Contract and Domain
- **Infrastructure** (`Anamnesis.Infrastructure`) — composition root; the only layer permitted to reference everything
- **Interface** (`Anamnesis.Interface.Website`) — UI layer; references UseCase Contracts and Domain
- **Test** (`<SubjectProjectName>.Test`) — one test project per subject assembly (e.g. `Anamnesis.Adapter.MedicalData.Nhs.Test`, `Anamnesis.Adapter.Llm.Ollama.Test`, `Anamnesis.UseCase.Conversation.Test`); architecture rules apply only to production assemblies

Without automated enforcement, new projects can silently break these boundaries.

## Goals / Non-Goals

**Goals:**
- Encode every structural rule as a failing xUnit test so CI catches violations immediately
- Cover dependency direction rules (no upward or cross-layer references)
- Cover project naming conventions (contract/implementation/test suffixes and segments)
- Cover namespace conventions (namespace must match assembly name)
- Zero friction for contributors — tests are in a single, self-contained project

**Non-Goals:**
- Class-level or method-level coding style enforcement (that belongs to Roslyn analysers)
- Runtime behaviour verification
- Enforcing internal visibility rules beyond what InternalsVisibleTo already handles

## Decisions

### Use NetArchTest.Rules (not a raw Roslyn or reflection approach)

NetArchTest is the de-facto .NET equivalent of ArchUnit. It reflects over compiled assemblies, so tests run without modifying any production project. The fluent API maps cleanly to the rules described in the architecture.

*Alternatives considered:*
- **Roslyn analysers** — powerful but require shipping as a NuGet package; overkill for intra-solution rules
- **Raw reflection** — achievable but verbose; NetArchTest already handles the plumbing

### One dedicated project: `Anamnesis.Architecture.Test`

All architecture tests live in one place, separate from unit tests, so they can be run independently (`dotnet test --filter "FullyQualifiedName~Architecture"`) without mixing concerns. The project references all production assemblies so NetArchTest can load every type.

*Alternative considered:* embedding tests inside an existing test project — rejected because it couples architecture rules to a specific layer and makes the dependency graph harder to reason about.

### Reference all production assemblies explicitly

NetArchTest needs to load types to evaluate rules. The architecture test project references every production assembly so that `.Types()` calls can enumerate the full solution. Test projects (`*.Test`) are deliberately excluded — architecture rules apply to production code only.

### Encode naming conventions as pattern-based rules

Project (assembly) naming rules:
- Adapter Contracts: name matches `Anamnesis.Adapter.*.Contract`
- Adapter Implementations: name matches `Anamnesis.Adapter.*.*` and does NOT end in `.Contract` or `.Test`; the two segments after `Adapter.` must be a domain concept (`<DomainConcept>`) and a technology name (`<Technology>`)
- UseCase Contracts: name matches `Anamnesis.UseCase.*.Contract`
- Test projects: name ends in `.Test` — kept out of architecture rules scope

## Risks / Trade-offs

- **Assembly-load brittleness** → NetArchTest loads assemblies from build output; if a project is not built, tests give misleading failures. Mitigation: CI always does a full `dotnet build` before `dotnet test`.
- **Rules lag behind new projects** → When a new project is added, architecture tests must be updated. Mitigation: the failing test is the signal — "you added a project; update the rules."
- **NetArchTest predicate expressiveness** → Some nuanced rules (e.g. "technology segment must not equal a domain concept") are hard to express fluently. Mitigation: fall back to custom predicates using `MeetCustomRule` when the built-in API is insufficient.

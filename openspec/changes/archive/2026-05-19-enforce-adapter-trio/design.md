## Context

`Anamnesis.Architecture.Test` already enforces layer dependency rules, naming segment counts, and namespace conventions using reflection-based assembly inspection. All production assemblies are listed explicitly in `SolutionAssemblies.cs`.

The existing tests work with a fixed set of known assemblies. The trio completeness test is different: it needs to reason about the *shape* of the adapter set as a whole, so it must discover assemblies dynamically rather than relying on the static registry.

The output directory already contains all referenced DLLs (production + their test projects are not in the output dir since only `Anamnesis.Architecture.Test` has project references to production assemblies). The adapter test projects (`Anamnesis.Adapter.MedicalData.Nhs.Test`, `Anamnesis.Adapter.Llm.Ollama.Test`) are **not** in `Architecture.Test`'s output directory — they are built separately and their DLLs are not copied there.

## Goals / Non-Goals

**Goals:**
- For each domain concept that has an adapter contract (`Anamnesis.Adapter.<DomainConcept>.Contract`), assert that at least one implementation (`Anamnesis.Adapter.<DomainConcept>.<Technology>`) and exactly one test per implementation (`Anamnesis.Adapter.<DomainConcept>.<Technology>.Test`) also exist.
- The check is structural: it uses the assembly naming convention to infer relationships, not project references.
- The test must be runnable as part of `dotnet test src/Anamnesis.Architecture.Test`.

**Non-Goals:**
- Does not check internal coverage or test quality.
- Does not enforce a 1:1 technology-to-contract ratio; one contract may have multiple implementations each with their own test.

## Decisions

**Discovery via filesystem scan, not static registry**

The existing `SolutionAssemblies.LoadAllInOutputDir()` scans `Anamnesis.*.dll` files in the output directory. However, adapter test assemblies are *not* copied into the architecture test's output directory (they have no project reference there).

Decision: add a new `ProjectReference` for each adapter test project in `Anamnesis.Architecture.Test.csproj`. This causes their DLLs to be copied to the output directory, making them discoverable by `LoadAllInOutputDir()`. This is the cleanest option — no extra search paths, no hardcoded paths, and the test runner continues to work with a single `dotnet test` invocation.

Alternative considered — hardcoded absolute paths: fragile, breaks on CI and other developer machines.

Alternative considered — a separate test runner script: adds tooling overhead and breaks the `dotnet test` pattern already established.

**Grouping logic**

Parse each discovered `Anamnesis.Adapter.*` assembly name by segment:
- 4 segments ending in `Contract` → contract assembly; domain concept = segment[2]
- 4 segments not ending in `Contract` or `Test` → implementation assembly; domain concept = segment[2], technology = segment[3]
- 5 segments ending in `Test` → test assembly; domain concept = segment[2], technology = segment[3]

For each domain concept with a contract, assert:
1. At least one implementation exists.
2. For each implementation, exactly one matching test assembly exists.

**No changes to `SolutionAssemblies.cs`**

The static registry is used by existing layer/naming/namespace tests which need only production assemblies. The trio test uses `LoadAllInOutputDir()` directly, keeping the two concerns separate.

## Risks / Trade-offs

[Risk] Adding test project references to `Architecture.Test.csproj` slightly increases build time. → Mitigation: negligible; test projects are small and already built as part of the solution.

[Risk] A domain concept with multiple technology implementations would require multiple test assemblies (one per implementation). The current codebase has one implementation per concept. → Mitigation: the assertion loops per-implementation, so it handles multiples correctly by design.

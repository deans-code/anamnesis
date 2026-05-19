## Context

The solution currently has four adapter projects that do not follow the domain-grouping convention already established by the two contract projects:

| Current name | New name |
|---|---|
| `Anamnesis.Adapter.Nhs` | `Anamnesis.Adapter.MedicalData.Nhs` |
| `Anamnesis.Adapter.Nhs.Test` | `Anamnesis.Adapter.MedicalData.Nhs.Test` |
| `Anamnesis.Adapter.Ollama` | `Anamnesis.Adapter.Llm.Ollama` |
| `Anamnesis.Adapter.Ollama.Test` | `Anamnesis.Adapter.Llm.Ollama.Test` |

Each rename touches: the directory name, the `.csproj` filename, the `<RootNamespace>` / `<AssemblyName>` (if set explicitly), every `namespace` declaration in source files, every `using` statement that references those namespaces, `InternalsVisibleTo` attributes, the `.sln` registration, and any `<ProjectReference>` paths in sibling projects.

## Goals / Non-Goals

**Goals:**
- All four projects renamed with consistent directory structure, project files, namespaces, and solution references
- All cross-project `<ProjectReference>` entries updated
- All `using` / `namespace` declarations updated throughout source files
- `InternalsVisibleTo` attribute in `Anamnesis.Adapter.Llm.Ollama` updated to reference the new test project name
- Solution builds and all tests pass after the rename

**Non-Goals:**
- Changing any runtime behaviour, interfaces, or business logic
- Renaming the two contract projects (already correctly named)
- Changing folder structure beyond the top-level `src/` directory rename

## Decisions

**Rename directories atomically via `git mv`**
Using `git mv` preserves history and avoids the directory appearing as a delete + add in the diff. Each of the four projects gets its own `git mv` for the directory, followed by the `.csproj` file rename within the new directory.

**Update namespaces globally via find-and-replace**
Root namespaces follow a strict prefix pattern (`Anamnesis.Adapter.Nhs.*` → `Anamnesis.Adapter.MedicalData.Nhs.*`, `Anamnesis.Adapter.Ollama.*` → `Anamnesis.Adapter.Llm.Ollama.*`). A targeted `sed` or IDE global replace across `src/` is sufficient; no manual file-by-file editing is needed. After replace, a build verifies nothing was missed.

**Update `.sln` project paths and GUIDs in-place**
The solution file references both the project name string and the relative path. Both must be updated; the GUID stays the same.

## Risks / Trade-offs

- **Missed reference** → Build failure immediately surfaces it; no silent runtime risk.
- **`InternalsVisibleTo` mismatch** → The Ollama adapter grants test access via assembly name; if the attribute is not updated, the test project cannot see internal types and tests fail at compile time.
- **Stale spec / tooling references** → Any ArchUnit test rules or architecture test strings hard-coded to old names will fail; they should be updated as part of this change.

## Migration Plan

1. `git mv` each source directory to its new name
2. Rename the `.csproj` file inside each moved directory
3. Update `<RootNamespace>` / `<AssemblyName>` in `.csproj` files if set explicitly
4. Global replace namespaces in all `.cs` files under `src/`
5. Update `InternalsVisibleTo` in `Anamnesis.Adapter.Llm.Ollama`
6. Update `anamnesis.sln` — project display names and relative paths
7. Update all `<ProjectReference>` entries in sibling `.csproj` files
8. `dotnet build` — fix any remaining errors
9. `dotnet test` — verify all tests pass

Rollback: revert all changes via git; no database or deployment state is affected.

## Why

`Anamnesis.Adapter.Nhs` was created without a matching Contract or Test project, breaking the structural convention established by `Anamnesis.Adapter.Ollama`. Consistency matters for navigability — every adapter in the solution should follow the same three-project pattern so developers know exactly where to look for the public surface, the implementation, and the tests.

## What Changes

- **New**: `Anamnesis.Adapter.Nhs.Contract` project added to the solution, containing a placeholder structure that mirrors the Ollama contract layout (ready to hold exception types or internal interfaces if the adapter grows).
- **New**: `Anamnesis.Adapter.Nhs.Test` project added to the solution, containing `NhsConditionReferenceServiceTests` that verify HTML parsing, caching, and HTTP failure behaviour using a mock HTTP handler.
- **Updated**: `Anamnesis.Adapter.Nhs.csproj` gains a reference to `Anamnesis.Adapter.Nhs.Contract` and adds `InternalsVisibleTo` for the test project.
- **Updated**: `anamnesis.sln` includes both new projects.
- No behavioural changes to the running application.

## Capabilities

### New Capabilities

- none (structural/consistency change; no user-facing or API behaviour changes)

### Modified Capabilities

- none

## Impact

- `src/Anamnesis.Adapter.Nhs.Contract/` — new project
- `src/Anamnesis.Adapter.Nhs.Test/` — new project
- `src/Anamnesis.Adapter.Nhs/Anamnesis.Adapter.Nhs.csproj` — adds contract reference and `InternalsVisibleTo`
- `anamnesis.sln` — two new projects registered

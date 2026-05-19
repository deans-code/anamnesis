## Why

Adapter project names currently omit the domain concept they implement (e.g. `Anamnesis.Adapter.Nhs`, `Anamnesis.Adapter.Ollama`), making it unclear at a glance which port/contract each adapter satisfies. Grouping by domain concept before implementation name brings the naming in line with the contract projects that already follow this convention (`Anamnesis.Adapter.MedicalData.Contract`, `Anamnesis.Adapter.Llm.Contract`).

## What Changes

- Rename `Anamnesis.Adapter.Nhs` → `Anamnesis.Adapter.MedicalData.Nhs` (directory, project file, root namespace)
- Rename `Anamnesis.Adapter.Nhs.Test` → `Anamnesis.Adapter.MedicalData.Nhs.Test` (directory, project file, root namespace)
- Rename `Anamnesis.Adapter.Ollama` → `Anamnesis.Adapter.Llm.Ollama` (directory, project file, root namespace)
- Rename `Anamnesis.Adapter.Ollama.Test` → `Anamnesis.Adapter.Llm.Ollama.Test` (directory, project file, root namespace)
- Update `.sln` project references and paths for all four projects
- Update all `using` / `namespace` declarations in source files to match the new names
- Update any cross-project `<ProjectReference>` entries that point to the renamed projects

The two contract projects (`Anamnesis.Adapter.MedicalData.Contract`, `Anamnesis.Adapter.Llm.Contract`) already follow the convention and are not renamed.

## Capabilities

### New Capabilities
<!-- none -->

### Modified Capabilities
- `nhs-adapter-structure`: Project names change — `Anamnesis.Adapter.Nhs` → `Anamnesis.Adapter.MedicalData.Nhs`, `Anamnesis.Adapter.Nhs.Test` → `Anamnesis.Adapter.MedicalData.Nhs.Test`
- `medical-reference-service`: Project name reference — `Anamnesis.Adapter.Nhs` → `Anamnesis.Adapter.MedicalData.Nhs`
- `condition-sidebar`: Project name reference — `Anamnesis.Adapter.Nhs` → `Anamnesis.Adapter.MedicalData.Nhs`
- `medical-data-adapter-contract`: Project name reference — `Anamnesis.Adapter.Nhs` → `Anamnesis.Adapter.MedicalData.Nhs`
- `conversation-engine-port`: Project name reference — `Anamnesis.Adapter.Ollama` → `Anamnesis.Adapter.Llm.Ollama`
- `infrastructure-composition-root`: Project name references — `Anamnesis.Adapter.Nhs` → `Anamnesis.Adapter.MedicalData.Nhs`, `Anamnesis.Adapter.Ollama` → `Anamnesis.Adapter.Llm.Ollama`
- `llm-adapter-contract`: Project name references — `Anamnesis.Adapter.Ollama` → `Anamnesis.Adapter.Llm.Ollama`, `Anamnesis.Adapter.Ollama.Test` → `Anamnesis.Adapter.Llm.Ollama.Test`

## Impact

- `src/Anamnesis.Adapter.Nhs/` and `src/Anamnesis.Adapter.Nhs.Test/` — directories and all contained files
- `src/Anamnesis.Adapter.Ollama/` and `src/Anamnesis.Adapter.Ollama.Test/` — directories and all contained files
- `anamnesis.sln` — project registrations and paths
- Any other project that holds a `<ProjectReference>` to the renamed projects

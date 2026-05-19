## Why

The architecture tests enforce layer dependency and naming rules but do not verify structural completeness: nothing currently checks that every adapter domain concept has all three expected projects (contract, implementation, implementation test). Without this, a developer could add a new adapter implementation without a contract, or skip the test project, and the CI suite would stay green.

## What Changes

- Add a new architecture test that asserts, for each distinct adapter domain concept (e.g. `Llm`, `MedicalData`) found in the output directory, exactly three assemblies exist: `Anamnesis.Adapter.<DomainConcept>.Contract`, `Anamnesis.Adapter.<DomainConcept>.<Technology>`, and `Anamnesis.Adapter.<DomainConcept>.<Technology>.Test`.
- The test loads all `Anamnesis.Adapter.*` assemblies present in the output directory (production + test) and validates the trio rule for each domain concept.

## Capabilities

### New Capabilities
- `adapter-trio-structure`: Each adapter domain concept must have exactly a contract assembly, one or more implementation assemblies, and a test assembly for each implementation.

### Modified Capabilities
- `architecture-tests`: New scenario added covering adapter structural completeness (trio enforcement rule).

## Impact

- `src/Anamnesis.Architecture.Test/` — new test class added
- `openspec/specs/architecture-tests/spec.md` — new scenario appended

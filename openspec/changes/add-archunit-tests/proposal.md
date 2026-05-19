## Why

As the solution grows, it is easy for contributors to accidentally introduce dependency violations or naming inconsistencies that erode the clean architecture and Ports & Adapters structure. Automated architecture tests that run in CI catch these regressions at the source, before they become costly to unwind.

## What Changes

- Add a new test project `Anamnesis.Architecture.Test` that uses NetArchTest to encode and enforce the structural rules of the solution.
- Tests cover layer dependency rules, project naming conventions, and interface/implementation placement rules.
- The project is added to the solution file and CI runs it alongside the existing unit test suite.

## Capabilities

### New Capabilities

- `architecture-tests`: A test project that verifies clean architecture dependency boundaries, Ports & Adapters project structure, and naming conventions (adapter domain-concept contracts, technology-named implementations, use case contracts, test project suffixes) are all upheld across the solution.

### Modified Capabilities

## Impact

- New project: `src/Anamnesis.Architecture.Test/`
- New entry in `anamnesis.sln`
- No changes to any existing production or test projects
- CI pipeline picks up the new test project automatically via `dotnet test`

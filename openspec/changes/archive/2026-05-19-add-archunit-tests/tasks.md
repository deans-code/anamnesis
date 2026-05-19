## 1. Project Setup

- [x] 1.1 Create `src/Anamnesis.Architecture.Test/` directory
- [x] 1.2 Create `Anamnesis.Architecture.Test.csproj` targeting `net10.0` with xUnit, `Microsoft.NET.Test.Sdk`, and `NetArchTest.Rules` package references
- [x] 1.3 Add `ProjectReference` entries for all production assemblies: `Domain`, `UseCase.Conversation`, `UseCase.Conversation.Contract`, `Adapter.Llm.Contract`, `Adapter.MedicalData.Contract`, `Adapter.Llm.Ollama`, `Adapter.MedicalData.Nhs`, `Infrastructure`, `Interface.Website`
- [x] 1.4 Add the new project to `anamnesis.sln`

## 2. Layer Dependency Tests

- [x] 2.1 Write test: `Domain` has no references to other `Anamnesis.*` assemblies
- [x] 2.2 Write test: `UseCase.*.Contract` assemblies do not reference UseCase implementations or any Adapter assembly
- [x] 2.3 Write test: `UseCase.*` (non-Contract) assemblies do not reference Adapter implementations, Infrastructure, or Interface
- [x] 2.4 Write test: `Adapter.*.Contract` assemblies do not reference UseCase or non-Contract Adapter assemblies
- [x] 2.5 Write test: `Adapter.*` (non-Contract) assemblies do not reference UseCase implementations, Infrastructure, Interface, or sibling Adapter implementations
- [x] 2.6 Write test: Only `Infrastructure` references both UseCase and Adapter implementations simultaneously

## 3. Naming Convention Tests

- [x] 3.1 Write test: Adapter Contract assembly names have exactly four dot-separated segments (`Anamnesis.Adapter.<Concept>.Contract`)
- [x] 3.2 Write test: Adapter implementation assembly names have exactly four dot-separated segments (`Anamnesis.Adapter.<DomainConcept>.<Technology>`)
- [x] 3.3 Write test: UseCase implementation assembly names have exactly three dot-separated segments (`Anamnesis.UseCase.<Name>`)
- [x] 3.4 Write test: UseCase Contract assembly names have exactly four dot-separated segments (`Anamnesis.UseCase.<Name>.Contract`)
- [x] 3.5 Write test: Test project assembly names end in `.Test`

## 4. Namespace Convention Tests

- [x] 4.1 Write test: Every type in each production assembly has a namespace that starts with the assembly name

## 5. Verify

- [x] 5.1 Run `dotnet build` and confirm the new project compiles cleanly
- [x] 5.2 Run `dotnet test src/Anamnesis.Architecture.Test` and confirm all tests pass against the current solution

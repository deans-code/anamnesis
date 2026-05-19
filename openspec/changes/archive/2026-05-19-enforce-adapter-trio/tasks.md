## 1. Project Setup

- [x] 1.1 Add `ProjectReference` entries for `Anamnesis.Adapter.MedicalData.Nhs.Test` and `Anamnesis.Adapter.Llm.Ollama.Test` in `Anamnesis.Architecture.Test.csproj` so their DLLs are copied to the output directory

## 2. Trio Completeness Test

- [x] 2.1 Write test: for every adapter domain concept that has a contract assembly, at least one implementation assembly (`Anamnesis.Adapter.<DomainConcept>.<Technology>`) exists
- [x] 2.2 Write test: for every adapter domain concept that has an implementation assembly, a contract assembly exists
- [x] 2.3 Write test: for every `Anamnesis.Adapter.<DomainConcept>.<Technology>` assembly, a matching `Anamnesis.Adapter.<DomainConcept>.<Technology>.Test` assembly exists

## 3. Verify

- [x] 3.1 Run `dotnet build src/Anamnesis.Architecture.Test` and confirm the project compiles cleanly
- [x] 3.2 Run `dotnet test src/Anamnesis.Architecture.Test` and confirm all tests pass, including the new trio tests

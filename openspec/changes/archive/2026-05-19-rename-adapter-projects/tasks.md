## 1. Rename NHS adapter directory and project file

- [x] 1.1 `git mv src/Anamnesis.Adapter.Nhs src/Anamnesis.Adapter.MedicalData.Nhs`
- [x] 1.2 `git mv src/Anamnesis.Adapter.MedicalData.Nhs/Anamnesis.Adapter.Nhs.csproj src/Anamnesis.Adapter.MedicalData.Nhs/Anamnesis.Adapter.MedicalData.Nhs.csproj`
- [x] 1.3 Update `<RootNamespace>` in `Anamnesis.Adapter.MedicalData.Nhs.csproj` from `Anamnesis.Adapter.Nhs` to `Anamnesis.Adapter.MedicalData.Nhs`

## 2. Rename NHS test adapter directory and project file

- [x] 2.1 `git mv src/Anamnesis.Adapter.Nhs.Test src/Anamnesis.Adapter.MedicalData.Nhs.Test`
- [x] 2.2 `git mv src/Anamnesis.Adapter.MedicalData.Nhs.Test/Anamnesis.Adapter.Nhs.Test.csproj src/Anamnesis.Adapter.MedicalData.Nhs.Test/Anamnesis.Adapter.MedicalData.Nhs.Test.csproj`
- [x] 2.3 Update `<RootNamespace>` in `Anamnesis.Adapter.MedicalData.Nhs.Test.csproj` from `Anamnesis.Adapter.Nhs.Test` to `Anamnesis.Adapter.MedicalData.Nhs.Test`
- [x] 2.4 Update `<ProjectReference>` in `Anamnesis.Adapter.MedicalData.Nhs.Test.csproj` to point to `..\Anamnesis.Adapter.MedicalData.Nhs\Anamnesis.Adapter.MedicalData.Nhs.csproj`

## 3. Rename Ollama adapter directory and project file

- [x] 3.1 `git mv src/Anamnesis.Adapter.Ollama src/Anamnesis.Adapter.Llm.Ollama`
- [x] 3.2 `git mv src/Anamnesis.Adapter.Llm.Ollama/Anamnesis.Adapter.Ollama.csproj src/Anamnesis.Adapter.Llm.Ollama/Anamnesis.Adapter.Llm.Ollama.csproj`
- [x] 3.3 Update `<RootNamespace>` in `Anamnesis.Adapter.Llm.Ollama.csproj` from `Anamnesis.Adapter.Ollama` to `Anamnesis.Adapter.Llm.Ollama`
- [x] 3.4 Update `<InternalsVisibleTo Include="Anamnesis.Adapter.Ollama.Test" />` to `<InternalsVisibleTo Include="Anamnesis.Adapter.Llm.Ollama.Test" />` in `Anamnesis.Adapter.Llm.Ollama.csproj`

## 4. Rename Ollama test adapter directory and project file

- [x] 4.1 `git mv src/Anamnesis.Adapter.Ollama.Test src/Anamnesis.Adapter.Llm.Ollama.Test`
- [x] 4.2 `git mv src/Anamnesis.Adapter.Llm.Ollama.Test/Anamnesis.Adapter.Ollama.Test.csproj src/Anamnesis.Adapter.Llm.Ollama.Test/Anamnesis.Adapter.Llm.Ollama.Test.csproj`
- [x] 4.3 Update `<RootNamespace>` in `Anamnesis.Adapter.Llm.Ollama.Test.csproj` from `Anamnesis.Adapter.Ollama.Test` to `Anamnesis.Adapter.Llm.Ollama.Test`
- [x] 4.4 Update `<ProjectReference>` in `Anamnesis.Adapter.Llm.Ollama.Test.csproj` to point to `..\Anamnesis.Adapter.Llm.Ollama\Anamnesis.Adapter.Llm.Ollama.csproj`

## 5. Update namespaces in source files

- [x] 5.1 In `src/Anamnesis.Adapter.MedicalData.Nhs/NhsConditionReferenceService.cs` replace `namespace Anamnesis.Adapter.Nhs` with `namespace Anamnesis.Adapter.MedicalData.Nhs`
- [x] 5.2 In `src/Anamnesis.Adapter.MedicalData.Nhs.Test/NhsConditionReferenceServiceTests.cs` replace `namespace Anamnesis.Adapter.Nhs.Test` with `namespace Anamnesis.Adapter.MedicalData.Nhs.Test` and `using Anamnesis.Adapter.Nhs` with `using Anamnesis.Adapter.MedicalData.Nhs`
- [x] 5.3 In all `.cs` files under `src/Anamnesis.Adapter.Llm.Ollama/` replace `namespace Anamnesis.Adapter.Ollama` with `namespace Anamnesis.Adapter.Llm.Ollama` (files: `IOllamaClient.cs`, `OllamaClient.cs`, `OllamaConversationEngine.cs`, `OllamaDtos.cs`, `OllamaSettings.cs`, `PromptTemplates.cs`)
- [x] 5.4 In all `.cs` files under `src/Anamnesis.Adapter.Llm.Ollama.Test/` replace `namespace Anamnesis.Adapter.Ollama.Test` with `namespace Anamnesis.Adapter.Llm.Ollama.Test` and `using Anamnesis.Adapter.Ollama` with `using Anamnesis.Adapter.Llm.Ollama`
- [x] 5.5 In `src/Anamnesis.Infrastructure/AnamnesisServiceCollectionExtensions.cs` replace `using Anamnesis.Adapter.Nhs` with `using Anamnesis.Adapter.MedicalData.Nhs` and `using Anamnesis.Adapter.Ollama` with `using Anamnesis.Adapter.Llm.Ollama`

## 6. Update solution file and cross-project references

- [x] 6.1 In `anamnesis.sln` update the project entry for `Anamnesis.Adapter.Nhs`: display name to `Anamnesis.Adapter.MedicalData.Nhs` and path to `src\Anamnesis.Adapter.MedicalData.Nhs\Anamnesis.Adapter.MedicalData.Nhs.csproj`
- [x] 6.2 In `anamnesis.sln` update the project entry for `Anamnesis.Adapter.Nhs.Test`: display name to `Anamnesis.Adapter.MedicalData.Nhs.Test` and path to `src\Anamnesis.Adapter.MedicalData.Nhs.Test\Anamnesis.Adapter.MedicalData.Nhs.Test.csproj`
- [x] 6.3 In `anamnesis.sln` update the project entry for `Anamnesis.Adapter.Ollama`: display name to `Anamnesis.Adapter.Llm.Ollama` and path to `src\Anamnesis.Adapter.Llm.Ollama\Anamnesis.Adapter.Llm.Ollama.csproj`
- [x] 6.4 In `anamnesis.sln` update the project entry for `Anamnesis.Adapter.Ollama.Test`: display name to `Anamnesis.Adapter.Llm.Ollama.Test` and path to `src\Anamnesis.Adapter.Llm.Ollama.Test\Anamnesis.Adapter.Llm.Ollama.Test.csproj`
- [x] 6.5 In `src/Anamnesis.Infrastructure/Anamnesis.Infrastructure.csproj` update `<ProjectReference>` for Nhs to `Anamnesis.Adapter.MedicalData.Nhs\Anamnesis.Adapter.MedicalData.Nhs.csproj` and for Ollama to `Anamnesis.Adapter.Llm.Ollama\Anamnesis.Adapter.Llm.Ollama.csproj`

## 7. Update the pending add-archunit-tests change

- [x] 7.1 In `openspec/changes/add-archunit-tests/tasks.md` task 1.3, replace `Adapter.Ollama` with `Adapter.Llm.Ollama` and `Adapter.Nhs` with `Adapter.MedicalData.Nhs`
- [x] 7.2 In `openspec/changes/add-archunit-tests/tasks.md` task 3.2, update the naming convention expectation from three segments (`Anamnesis.Adapter.<Technology>`) to four segments (`Anamnesis.Adapter.<Domain>.<Technology>`) to match the new convention

## 8. Verify

- [x] 8.1 Run `dotnet build anamnesis.sln` and confirm zero errors
- [x] 8.2 Run `dotnet test anamnesis.sln` and confirm all tests pass

## 1. Create Anamnesis.Adapter.Llm.Contract

- [x] 1.1 Create `src/Anamnesis.Adapter.Llm.Contract/` and add `Anamnesis.Adapter.Llm.Contract.csproj` targeting `net10.0` with nullable and implicit usings enabled. No project references needed. Set `RootNamespace` to `Anamnesis.Adapter.Llm.Contract`.
- [x] 1.2 Create `LlmUnavailableException.cs` in `Anamnesis.Adapter.Llm.Contract` with namespace `Anamnesis.Adapter.Llm.Contract`, matching the structure of the existing `OllamaUnavailableException` (single constructor taking `string message` and `Exception innerException`).
- [x] 1.3 Create `RateLimitExceededException.cs` in `Anamnesis.Adapter.Llm.Contract` with namespace `Anamnesis.Adapter.Llm.Contract`, matching the existing `RateLimitExceededException` (two constructors: `string message`, and `string message, Exception innerException`).
- [x] 1.4 Add `Anamnesis.Adapter.Llm.Contract` to `anamnesis.sln` using `dotnet sln add`.

## 2. Move IOllamaClient into Anamnesis.Adapter.Ollama

- [x] 2.1 Copy `IOllamaClient.cs` from `src/Anamnesis.Adapter.Ollama.Contract/` into `src/Anamnesis.Adapter.Ollama/`. Update its namespace to `Anamnesis.Adapter.Ollama` and change the access modifier from `public` to `internal`.
- [x] 2.2 Add `<InternalsVisibleTo Include="Anamnesis.Adapter.Ollama.Test" />` to `Anamnesis.Adapter.Ollama.csproj`.

## 3. Update Anamnesis.Adapter.Ollama

- [x] 3.1 In `Anamnesis.Adapter.Ollama.csproj`, replace the project reference to `Anamnesis.Adapter.Ollama.Contract` with a reference to `Anamnesis.Adapter.Llm.Contract`.
- [x] 3.2 In `OllamaClient.cs`, replace `using Anamnesis.Adapter.Ollama.Contract;` with `using Anamnesis.Adapter.Llm.Contract;`. Replace all occurrences of `OllamaUnavailableException` with `LlmUnavailableException`.
- [x] 3.3 In `OllamaConversationEngine.cs`, replace `using Anamnesis.Adapter.Ollama.Contract;` with `using Anamnesis.Adapter.Llm.Contract;`. Replace all occurrences of `OllamaUnavailableException` with `LlmUnavailableException`. Remove any now-redundant using for the old contract namespace.

## 4. Update Anamnesis.Adapter.Ollama.Test

- [x] 4.1 In `Anamnesis.Adapter.Ollama.Test.csproj`, replace the project reference to `Anamnesis.Adapter.Ollama.Contract` with a reference to `Anamnesis.Adapter.Llm.Contract`.
- [x] 4.2 In `OllamaClientTests.cs`, replace `using Anamnesis.Adapter.Ollama.Contract;` with `using Anamnesis.Adapter.Llm.Contract;`. Rename all references to `OllamaUnavailableException` → `LlmUnavailableException` and update any test method names that include `OllamaUnavailable` → `LlmUnavailable`.
- [x] 4.3 In `OllamaConversationEngineTests.cs`, replace `using Anamnesis.Adapter.Ollama.Contract;` with `using Anamnesis.Adapter.Ollama;` (for `IOllamaClient`) and `using Anamnesis.Adapter.Llm.Contract;` (for exceptions). Replace `OllamaUnavailableException` with `LlmUnavailableException` throughout.

## 5. Update Anamnesis.Infrastructure

- [ ] 5.1 In `Anamnesis.Infrastructure.csproj`, replace the project reference to `Anamnesis.Adapter.Ollama.Contract` with a reference to `Anamnesis.Adapter.Llm.Contract`.
- [ ] 5.2 In `AnamnesisServiceCollectionExtensions.cs`, replace `using Anamnesis.Adapter.Ollama.Contract;` with `using Anamnesis.Adapter.Ollama;` (since `IOllamaClient` now lives there).

## 6. Delete Anamnesis.Adapter.Ollama.Contract

- [x] 6.1 Remove `Anamnesis.Adapter.Ollama.Contract` from `anamnesis.sln` using `dotnet sln remove`.
- [x] 6.2 Delete the `src/Anamnesis.Adapter.Ollama.Contract/` directory.

## 7. Verify

- [x] 7.1 Build the full solution and confirm zero errors.
- [x] 7.2 Run the full test suite and confirm all tests pass.

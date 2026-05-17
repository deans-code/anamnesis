## 1. Define the IMedicalReferenceService port

- [x] 1.1 Add `IMedicalReferenceService` interface to `Anamnesis.UseCase.Conversation.Contract` with two methods: `Task<IReadOnlyList<RelatedCondition>> GetConditionsAsync()` and `Task<IReadOnlyList<RelatedCondition>> GetSymptomsAsync()`.

## 2. Create the Anamnesis.Adapter.Nhs project

- [x] 2.1 Create the directory `src/Anamnesis.Adapter.Nhs/` and add `Anamnesis.Adapter.Nhs.csproj` targeting `net10.0` with nullable and implicit usings enabled, referencing `Anamnesis.Domain` and `Anamnesis.UseCase.Conversation.Contract`.
- [x] 2.2 Add the new project to `anamnesis.sln` using `dotnet sln add`.
- [x] 2.3 Create `NhsConditionReferenceService.cs` in `Anamnesis.Adapter.Nhs` implementing `IMedicalReferenceService`. Move the HTTP-fetch, HTML-parse, and cache logic from `NhsIndexService` into this class. The NHS index URLs, link prefixes, caching, and `SemaphoreSlim` fetch lock are internal implementation details of this class. The class MUST NOT expose `NhsIndexType` or any other NHS-specific type in its public surface.

## 3. Update the use case

- [x] 3.1 Replace the `INhsIndexService` constructor parameter in `ConversationService` with `IMedicalReferenceService`.
- [x] 3.2 Update `GetRelatedConditionsAsync` in `ConversationService` to call `_medicalReferenceService.GetConditionsAsync()` and `GetSymptomsAsync()` and pass the results directly to `_engine.GetRelatedConditionsAsync(conditions, symptoms)`. Remove all references to `NhsIndexType` and the per-name `GetUrl` lookup.

## 4. Delete obsolete types

- [x] 4.1 Delete `src/Anamnesis.UseCase.Conversation.Contract/INhsIndexService.cs`.
- [x] 4.2 Delete `src/Anamnesis.UseCase.Conversation.Contract/NhsIndexType.cs`.
- [x] 4.3 Delete `src/Anamnesis.Interface.Website/NhsIndexService.cs`.

## 5. Update project references and DI

- [x] 5.1 Add a project reference from `Anamnesis.Interface.Website` to `Anamnesis.Adapter.Nhs` in its `.csproj`; remove any now-unused reference to types from the deleted files.
- [x] 5.2 In `Program.cs`, replace the `INhsIndexService` → `NhsIndexService` singleton registration with `IMedicalReferenceService` → `NhsConditionReferenceService` (keep `Singleton` lifetime and the named `"nhs"` HTTP client).

## 6. Update tests

- [x] 6.1 Update `ConversationServiceTests` to mock `IMedicalReferenceService` instead of `INhsIndexService`: replace `GetNamesAsync`/`GetUrl` stubs with `GetConditionsAsync`/`GetSymptomsAsync` returning `IReadOnlyList<RelatedCondition>` directly.
- [x] 6.2 Remove the `Anamnesis.Adapter.Ollama.Contract` reference from `Anamnesis.UseCase.Conversation.Test.csproj` if it was still present (verify — it may have been cleaned up in the prior change).
- [x] 6.3 Run the full test suite and confirm all tests pass.

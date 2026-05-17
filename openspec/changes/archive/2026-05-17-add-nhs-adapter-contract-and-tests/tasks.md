## 1. Add Anamnesis.Adapter.Nhs.Contract

- [x] 1.1 Create `src/Anamnesis.Adapter.Nhs.Contract/` and add `Anamnesis.Adapter.Nhs.Contract.csproj` targeting `net10.0` with nullable and implicit usings enabled. No project references or code files are needed at this time.
- [x] 1.2 Add `Anamnesis.Adapter.Nhs.Contract` to `anamnesis.sln` using `dotnet sln add`.
- [x] 1.3 Add a project reference from `Anamnesis.Adapter.Nhs` to `Anamnesis.Adapter.Nhs.Contract` in its `.csproj`.

## 2. Add Anamnesis.Adapter.Nhs.Test

- [x] 2.1 Create `src/Anamnesis.Adapter.Nhs.Test/` and add `Anamnesis.Adapter.Nhs.Test.csproj` targeting `net10.0` with `IsPackable` false, referencing `Anamnesis.Adapter.Nhs` and `Anamnesis.Adapter.Nhs.Contract`. Include xunit, NSubstitute, and `Microsoft.Extensions.Logging.Abstractions` package references (matching the Ollama test project pattern).
- [x] 2.2 Add `Anamnesis.Adapter.Nhs.Test` to `anamnesis.sln` using `dotnet sln add`.
- [x] 2.3 Create `NhsConditionReferenceServiceTests.cs` in `Anamnesis.Adapter.Nhs.Test`. Define a local `MockHttpMessageHandler` (same pattern as in `OllamaClientTests`). Write tests covering:
  - `GetConditionsAsync_ReturnsEntries_WhenHtmlContainsNhsConditionLinks`: serve HTML with a `/conditions/` anchor, assert the returned list contains the entry with the correct name and absolute URL.
  - `GetSymptomsAsync_ReturnsEntries_WhenHtmlContainsNhsSymptomsLinks`: serve HTML with a `/symptoms/` anchor, assert the returned list contains the entry.
  - `GetConditionsAsync_CachesResult_SecondCallMakesNoHttpRequest`: use a call-counting handler, call `GetConditionsAsync()` twice, assert exactly one HTTP request was made.
  - `GetConditionsAsync_ReturnsEmptyList_WhenHttpRequestFails`: use a `FailingHttpMessageHandler`, assert the result is an empty list and no exception is thrown.
  - `GetConditionsAsync_ReturnsEmptyList_WhenHtmlContainsNoMatchingLinks`: serve HTML with no `/conditions/` anchors, assert empty list returned.

## 3. Verify

- [x] 3.1 Build the full solution and confirm zero errors.
- [x] 3.2 Run the full test suite and confirm all tests pass including the new NHS adapter tests.

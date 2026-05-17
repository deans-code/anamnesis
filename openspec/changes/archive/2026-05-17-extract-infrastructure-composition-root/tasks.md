## 1. Create Anamnesis.Adapter.MedicalData.Contract

- [x] 1.1 Create `src/Anamnesis.Adapter.MedicalData.Contract/` and add `Anamnesis.Adapter.MedicalData.Contract.csproj` targeting `net10.0` with nullable and implicit usings enabled, referencing `Anamnesis.Domain`. Set `RootNamespace` to `Anamnesis.Adapter.MedicalData.Contract`.
- [x] 1.2 Move `IMedicalReferenceService.cs` from `src/Anamnesis.UseCase.Conversation.Contract/` into `src/Anamnesis.Adapter.MedicalData.Contract/`, updating its namespace to `Anamnesis.Adapter.MedicalData.Contract`.
- [x] 1.3 Add `Anamnesis.Adapter.MedicalData.Contract` to `anamnesis.sln` using `dotnet sln add`.

## 2. Update Anamnesis.UseCase.Conversation.Contract

- [x] 2.1 Remove the `IMedicalReferenceService.cs` file from `src/Anamnesis.UseCase.Conversation.Contract/` (already moved in 1.2).
- [x] 2.2 Remove the project reference to `Anamnesis.Domain` from `Anamnesis.UseCase.Conversation.Contract.csproj` if `IMedicalReferenceService` was the only file using domain types. Verify by checking remaining files — keep the reference if `AuditEntry.cs` or other files still use `Domain` types.

## 3. Update Anamnesis.UseCase.Conversation

- [x] 3.1 Add a project reference to `Anamnesis.Adapter.MedicalData.Contract` in `Anamnesis.UseCase.Conversation.csproj`.
- [x] 3.2 Add `using Anamnesis.Adapter.MedicalData.Contract;` to `ConversationService.cs` and verify it compiles with `IMedicalReferenceService` resolved from the new namespace.

## 4. Update Anamnesis.Adapter.Nhs

- [x] 4.1 In `Anamnesis.Adapter.Nhs.csproj`, replace the project reference to `Anamnesis.UseCase.Conversation.Contract` with a reference to `Anamnesis.Adapter.MedicalData.Contract`.
- [x] 4.2 In `NhsConditionReferenceService.cs`, replace `using Anamnesis.UseCase.Conversation.Contract;` with `using Anamnesis.Adapter.MedicalData.Contract;`.

## 5. Update Anamnesis.Adapter.Nhs.Test

- [x] 5.1 In `Anamnesis.Adapter.Nhs.Test.csproj`, replace the project reference to `Anamnesis.Adapter.Nhs.Contract` with a reference to `Anamnesis.Adapter.MedicalData.Contract`.

## 6. Remove Anamnesis.Adapter.Nhs.Contract

- [x] 6.1 Remove `Anamnesis.Adapter.Nhs.Contract` from `anamnesis.sln` using `dotnet sln remove`.
- [x] 6.2 Delete the `src/Anamnesis.Adapter.Nhs.Contract/` directory.
- [x] 6.3 Remove the project reference to `Anamnesis.Adapter.Nhs.Contract` from `Anamnesis.Adapter.Nhs.csproj`.

## 7. Create Anamnesis.Infrastructure

- [x] 7.1 Create `src/Anamnesis.Infrastructure/` and add `Anamnesis.Infrastructure.csproj` targeting `net10.0` with nullable and implicit usings enabled. Add project references to `Anamnesis.Domain`, `Anamnesis.UseCase.Conversation`, `Anamnesis.UseCase.Conversation.Contract`, `Anamnesis.Adapter.MedicalData.Contract`, `Anamnesis.Adapter.Nhs`, `Anamnesis.Adapter.Ollama`, and `Anamnesis.Adapter.Ollama.Contract`. Add the `Microsoft.Extensions.Http.Resilience` and `System.Threading.RateLimiting` package references needed for HTTP client and rate limiter configuration.
- [x] 7.2 Add `Anamnesis.Infrastructure` to `anamnesis.sln` using `dotnet sln add`.
- [x] 7.3 Create `src/Anamnesis.Infrastructure/AnamnesisServiceCollectionExtensions.cs` containing a static class with `AddAnamnesis(this IServiceCollection services, IConfiguration configuration)`. Move all adapter registration logic from `Interface.Website/Program.cs` into this method: `OllamaSettings` options binding and validation, `IOllamaClient`/`OllamaClient` typed HTTP client with resilience handler and rate limiter, NHS named HTTP client, `IMedicalReferenceService`/`NhsConditionReferenceService` singleton, `IAuditLogger`/`FileAuditLogger` scoped, `IConversationEngine`/`OllamaConversationEngine` scoped, `IConversationService`/`ConversationService` scoped, and `AuditLogging` configuration section binding.

## 8. Update Anamnesis.Interface.Website

- [x] 8.1 In `Anamnesis.Interface.Website.csproj`, remove project references to `Anamnesis.Adapter.Nhs`, `Anamnesis.Adapter.Ollama`, and `Anamnesis.Adapter.Ollama.Contract`. Add a project reference to `Anamnesis.Infrastructure`.
- [x] 8.2 In `Program.cs`, replace all adapter registration code with a single call to `builder.Services.AddAnamnesis(builder.Configuration)`. Retain only Blazor/MudBlazor registration (`AddRazorComponents`, `AddInteractiveServerComponents`, `AddMudServices`) and the app pipeline setup (`UseStaticFiles`, `UseAntiforgery`, `MapRazorComponents`).

## 9. Verify

- [x] 9.1 Build the full solution and confirm zero errors.
- [x] 9.2 Run the full test suite and confirm all tests pass.

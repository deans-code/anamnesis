## Why

The use case layer currently depends on `INhsIndexService` and `NhsIndexType` — NHS-specific types defined in the use case contract — which couples the conversation use case to a particular data source. The use case should only know that it can obtain a catalogue of known conditions and symptoms; the fact that this data comes from the NHS website is an infrastructure detail belonging in an adapter.

## What Changes

- **BREAKING**: `INhsIndexService` removed from `Anamnesis.UseCase.Conversation.Contract`; replaced by `IMedicalReferenceService`.
- **BREAKING**: `NhsIndexType` enum removed from `Anamnesis.UseCase.Conversation.Contract`.
- **New**: `IMedicalReferenceService` interface added to `Anamnesis.UseCase.Conversation.Contract` with two methods: `GetConditionsAsync()` and `GetSymptomsAsync()`, both returning `Task<IReadOnlyList<RelatedCondition>>`.
- **New**: `Anamnesis.Adapter.Nhs` project created to house all NHS-specific infrastructure.
- **New**: `NhsConditionReferenceService` class created in `Anamnesis.Adapter.Nhs`, implementing `IMedicalReferenceService`. Internally retains the existing HTML-fetch-and-parse logic from `NhsIndexService`.
- **Removed**: `NhsIndexService` deleted from `Anamnesis.Interface.Website`.
- **Simplified**: `ConversationService` calls `IMedicalReferenceService.GetConditionsAsync()` and `GetSymptomsAsync()` directly, receiving `IReadOnlyList<RelatedCondition>` — no mention of NHS or index types.
- **Updated**: DI registration in `Program.cs` wires `NhsConditionReferenceService` to `IMedicalReferenceService`.
- **Updated**: `ConversationServiceTests` updated to mock `IMedicalReferenceService`.

## Capabilities

### New Capabilities

- `medical-reference-service`: The technology-agnostic port through which the use case retrieves the catalogue of known conditions and symptoms with their reference URLs.

### Modified Capabilities

- none (external user-facing behaviour is unchanged; this is a pure architectural refactoring)

## Impact

- `Anamnesis.UseCase.Conversation.Contract` — `INhsIndexService.cs` and `NhsIndexType.cs` deleted; `IMedicalReferenceService.cs` added.
- `Anamnesis.UseCase.Conversation` — `ConversationService.cs` updated to use `IMedicalReferenceService`.
- `Anamnesis.UseCase.Conversation.Test` — `ConversationServiceTests.cs` updated to mock `IMedicalReferenceService`.
- `Anamnesis.Interface.Website` — `NhsIndexService.cs` deleted; project reference to `Anamnesis.Adapter.Nhs` added.
- `Anamnesis.Adapter.Nhs` — new project; contains `NhsConditionReferenceService.cs` and its `.csproj`.
- `anamnesis.sln` — new project added.
- `Program.cs` — DI registration updated.

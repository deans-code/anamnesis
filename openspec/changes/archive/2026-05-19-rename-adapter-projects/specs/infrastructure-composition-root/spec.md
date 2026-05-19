## MODIFIED Requirements

### Requirement: The web interface has no direct dependency on adapter projects
`Anamnesis.Interface.Website` SHALL NOT reference `Anamnesis.Adapter.MedicalData.Nhs`, `Anamnesis.Adapter.Llm.Ollama`, or `Anamnesis.Adapter.Llm.Contract` as project dependencies. The web interface SHALL depend only on `Anamnesis.UseCase.Conversation.Contract`, `Anamnesis.Domain`, and `Anamnesis.Infrastructure`.

#### Scenario: Website project references contain no adapter projects
- **WHEN** the project references of `Anamnesis.Interface.Website` are inspected
- **THEN** they SHALL NOT include any `Adapter.*` project other than `Anamnesis.Infrastructure`

#### Scenario: Website Program.cs delegates all registration to infrastructure
- **WHEN** `Program.cs` in `Anamnesis.Interface.Website` is inspected
- **THEN** adapter-specific types (e.g. `OllamaConversationEngine`, `NhsConditionReferenceService`) SHALL NOT be referenced directly; all registration SHALL be delegated to `AddAnamnesis`

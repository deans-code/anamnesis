## ADDED Requirements

### Requirement: A dedicated infrastructure project owns all adapter wiring
The solution SHALL contain a project `Anamnesis.Infrastructure` that is responsible for registering all adapter implementations into the DI container. It SHALL expose a single extension method `AddAnamnesis(this IServiceCollection, IConfiguration)` that registers all services required for the application to function. No adapter registration SHALL occur in `Anamnesis.Interface.Website`.

#### Scenario: Infrastructure project exists in the solution
- **WHEN** the solution file is inspected
- **THEN** `Anamnesis.Infrastructure` SHALL be registered as a project

#### Scenario: All adapter services are registered via the infrastructure project
- **WHEN** `AddAnamnesis` is called
- **THEN** `IConversationEngine`, `IMedicalReferenceService`, `IAuditLogger`, and `IConversationService` SHALL all be resolvable from the resulting service provider

### Requirement: The web interface has no direct dependency on adapter projects
`Anamnesis.Interface.Website` SHALL NOT reference `Anamnesis.Adapter.Nhs`, `Anamnesis.Adapter.Ollama`, or `Anamnesis.Adapter.Ollama.Contract` as project dependencies. The web interface SHALL depend only on `Anamnesis.UseCase.Conversation.Contract`, `Anamnesis.Domain`, and `Anamnesis.Infrastructure`.

#### Scenario: Website project references contain no adapter projects
- **WHEN** the project references of `Anamnesis.Interface.Website` are inspected
- **THEN** they SHALL NOT include any `Adapter.*` project other than `Anamnesis.Infrastructure`

#### Scenario: Website Program.cs delegates all registration to infrastructure
- **WHEN** `Program.cs` in `Anamnesis.Interface.Website` is inspected
- **THEN** adapter-specific types (e.g. `OllamaConversationEngine`, `NhsConditionReferenceService`) SHALL NOT be referenced directly; all registration SHALL be delegated to `AddAnamnesis`

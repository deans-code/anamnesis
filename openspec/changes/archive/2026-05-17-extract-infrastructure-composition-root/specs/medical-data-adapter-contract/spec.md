## ADDED Requirements

### Requirement: Medical data adapter contract is defined independently of the use case
The solution SHALL contain a project `Anamnesis.Adapter.MedicalData.Contract` that defines `IMedicalReferenceService`. This project SHALL reference only `Anamnesis.Domain`. It SHALL NOT reference any use case project or any concrete adapter project. `IMedicalReferenceService` SHALL NOT appear in `Anamnesis.UseCase.Conversation.Contract`.

#### Scenario: IMedicalReferenceService is in the adapter contract project
- **WHEN** the solution is inspected
- **THEN** `IMedicalReferenceService` SHALL be defined in `Anamnesis.Adapter.MedicalData.Contract` and SHALL NOT be defined in `Anamnesis.UseCase.Conversation.Contract`

#### Scenario: Adapter contract has no use case dependency
- **WHEN** the project references of `Anamnesis.Adapter.MedicalData.Contract` are inspected
- **THEN** it SHALL reference only `Anamnesis.Domain` and SHALL NOT reference any `UseCase` project

### Requirement: The NHS adapter implements the medical data adapter contract
`Anamnesis.Adapter.Nhs` SHALL reference `Anamnesis.Adapter.MedicalData.Contract` and implement `IMedicalReferenceService`. It SHALL NOT reference `Anamnesis.UseCase.Conversation.Contract`.

#### Scenario: NHS adapter references adapter contract, not use case contract
- **WHEN** the project references of `Anamnesis.Adapter.Nhs` are inspected
- **THEN** it SHALL reference `Anamnesis.Adapter.MedicalData.Contract` and SHALL NOT reference `Anamnesis.UseCase.Conversation.Contract`

### Requirement: The use case implementation references the adapter contract directly
`Anamnesis.UseCase.Conversation` SHALL reference `Anamnesis.Adapter.MedicalData.Contract` directly in order to use `IMedicalReferenceService`. This reference SHALL appear in the implementation project, not in `Anamnesis.UseCase.Conversation.Contract`.

#### Scenario: Use case implementation has explicit adapter contract reference
- **WHEN** the project references of `Anamnesis.UseCase.Conversation` are inspected
- **THEN** it SHALL include a reference to `Anamnesis.Adapter.MedicalData.Contract`

#### Scenario: Use case contract has no adapter contract reference
- **WHEN** the project references of `Anamnesis.UseCase.Conversation.Contract` are inspected
- **THEN** it SHALL NOT reference `Anamnesis.Adapter.MedicalData.Contract`

## MODIFIED Requirements

### Requirement: The NHS adapter implements the medical data adapter contract
`Anamnesis.Adapter.MedicalData.Nhs` SHALL reference `Anamnesis.Adapter.MedicalData.Contract` and implement `IMedicalReferenceService`. It SHALL NOT reference `Anamnesis.UseCase.Conversation.Contract`.

#### Scenario: NHS adapter references adapter contract, not use case contract
- **WHEN** the project references of `Anamnesis.Adapter.MedicalData.Nhs` are inspected
- **THEN** it SHALL reference `Anamnesis.Adapter.MedicalData.Contract` and SHALL NOT reference `Anamnesis.UseCase.Conversation.Contract`

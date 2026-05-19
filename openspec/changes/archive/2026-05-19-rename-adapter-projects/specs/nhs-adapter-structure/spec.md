## MODIFIED Requirements

### Requirement: NHS adapter follows the three-project adapter pattern
The solution SHALL contain three projects for the NHS adapter: `Anamnesis.Adapter.MedicalData.Contract` (the technology-agnostic port definition), `Anamnesis.Adapter.MedicalData.Nhs` (the implementation), and `Anamnesis.Adapter.MedicalData.Nhs.Test` (the test project), mirroring the structure of the Ollama adapter.

#### Scenario: Medical data adapter contract project exists in the solution
- **WHEN** the solution file is inspected
- **THEN** `Anamnesis.Adapter.MedicalData.Contract` SHALL be registered as a project

#### Scenario: NHS test project exists in the solution
- **WHEN** the solution file is inspected
- **THEN** `Anamnesis.Adapter.MedicalData.Nhs.Test` SHALL be registered as a project

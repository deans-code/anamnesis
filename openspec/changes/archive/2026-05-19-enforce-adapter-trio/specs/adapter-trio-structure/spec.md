## ADDED Requirements

### Requirement: Each adapter domain concept has a complete project trio
For every adapter domain concept present in the solution, there SHALL be exactly three project types: a contract project (`Anamnesis.Adapter.<DomainConcept>.Contract`), at least one implementation project (`Anamnesis.Adapter.<DomainConcept>.<Technology>`), and exactly one test project per implementation (`Anamnesis.Adapter.<DomainConcept>.<Technology>.Test`).

#### Scenario: Adapter domain concept has a contract project
- **WHEN** the architecture tests are run
- **THEN** every adapter domain concept that has an implementation assembly also has a corresponding `Anamnesis.Adapter.<DomainConcept>.Contract` assembly

#### Scenario: Adapter domain concept has at least one implementation project
- **WHEN** the architecture tests are run
- **THEN** every adapter domain concept that has a contract assembly also has at least one `Anamnesis.Adapter.<DomainConcept>.<Technology>` assembly

#### Scenario: Each adapter implementation has a matching test project
- **WHEN** the architecture tests are run
- **THEN** every `Anamnesis.Adapter.<DomainConcept>.<Technology>` assembly has a corresponding `Anamnesis.Adapter.<DomainConcept>.<Technology>.Test` assembly

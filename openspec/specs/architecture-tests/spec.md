### Requirement: Domain layer has no outward dependencies
The `Anamnesis.Domain` assembly SHALL NOT reference any other `Anamnesis.*` assembly.

#### Scenario: Domain references no other Anamnesis assembly
- **WHEN** the architecture tests are run
- **THEN** no type in `Anamnesis.Domain` depends on a type in any other `Anamnesis.*` namespace

---

### Requirement: UseCase Contract depends only on Domain
Each `Anamnesis.UseCase.*.Contract` assembly SHALL only reference `Anamnesis.Domain` within the solution.

#### Scenario: UseCase Contract has no UseCase implementation dependency
- **WHEN** the architecture tests are run
- **THEN** no type in a `Anamnesis.UseCase.*.Contract` assembly depends on a type in the matching `Anamnesis.UseCase.*` (non-Contract) assembly

#### Scenario: UseCase Contract has no Adapter dependency
- **WHEN** the architecture tests are run
- **THEN** no type in a `Anamnesis.UseCase.*.Contract` assembly depends on a type in any `Anamnesis.Adapter.*` assembly

---

### Requirement: UseCase depends only on Domain and its own Contract
Each `Anamnesis.UseCase.<Name>` assembly SHALL only reference `Anamnesis.Domain` and `Anamnesis.UseCase.<Name>.Contract` within the solution (plus any Adapter Contracts it drives through).

#### Scenario: UseCase has no Adapter implementation dependency
- **WHEN** the architecture tests are run
- **THEN** no type in a `Anamnesis.UseCase.*` (non-Contract) assembly depends on a type in any `Anamnesis.Adapter.*` assembly that is not a `.Contract` assembly

#### Scenario: UseCase has no Infrastructure dependency
- **WHEN** the architecture tests are run
- **THEN** no type in a `Anamnesis.UseCase.*` assembly depends on a type in `Anamnesis.Infrastructure`

#### Scenario: UseCase has no Interface dependency
- **WHEN** the architecture tests are run
- **THEN** no type in a `Anamnesis.UseCase.*` assembly depends on a type in `Anamnesis.Interface.*`

---

### Requirement: Adapter Contract depends only on Domain
Each `Anamnesis.Adapter.*.Contract` assembly SHALL NOT reference any other `Anamnesis.*` assembly except `Anamnesis.Domain`.

#### Scenario: Adapter Contract has no UseCase dependency
- **WHEN** the architecture tests are run
- **THEN** no type in a `Anamnesis.Adapter.*.Contract` assembly depends on a type in any `Anamnesis.UseCase.*` assembly

#### Scenario: Adapter Contract has no Adapter implementation dependency
- **WHEN** the architecture tests are run
- **THEN** no type in a `Anamnesis.Adapter.*.Contract` assembly depends on a type in any `Anamnesis.Adapter.*` assembly that is not a `.Contract`

---

### Requirement: Adapter implementation depends only on its Contract and Domain
Each `Anamnesis.Adapter.<DomainConcept>.<Technology>` assembly SHALL only reference `Anamnesis.Domain` and one or more `Anamnesis.Adapter.*.Contract` assemblies within the solution.

#### Scenario: Adapter has no UseCase implementation dependency
- **WHEN** the architecture tests are run
- **THEN** no type in a `Anamnesis.Adapter.*` (non-Contract) assembly depends on a type in any `Anamnesis.UseCase.*` (non-Contract) assembly

#### Scenario: Adapter has no Infrastructure dependency
- **WHEN** the architecture tests are run
- **THEN** no type in a `Anamnesis.Adapter.*` (non-Contract) assembly depends on a type in `Anamnesis.Infrastructure`

#### Scenario: Adapter has no Interface dependency
- **WHEN** the architecture tests are run
- **THEN** no type in a `Anamnesis.Adapter.*` (non-Contract) assembly depends on a type in `Anamnesis.Interface.*`

#### Scenario: Adapter has no sibling Adapter implementation dependency
- **WHEN** the architecture tests are run
- **THEN** no type in a `Anamnesis.Adapter.*` (non-Contract) assembly depends on a type in another `Anamnesis.Adapter.*` (non-Contract) assembly

---

### Requirement: Infrastructure is the only composition root
`Anamnesis.Infrastructure` SHALL be the only assembly permitted to reference both UseCase implementations and Adapter implementations simultaneously.

#### Scenario: No layer other than Infrastructure references both a UseCase and an Adapter implementation
- **WHEN** the architecture tests are run
- **THEN** only `Anamnesis.Infrastructure` has direct references to both `Anamnesis.UseCase.*` (non-Contract) and `Anamnesis.Adapter.*` (non-Contract) assemblies

---

### Requirement: Adapter Contract projects are named with a domain-concept segment
Every `Anamnesis.Adapter.*.Contract` project name SHALL have exactly the form `Anamnesis.Adapter.<DomainConcept>.Contract`, where `<DomainConcept>` is a single PascalCase segment representing a domain concept (not a technology name).

#### Scenario: Adapter Contract name follows the domain-concept pattern
- **WHEN** the architecture tests are run
- **THEN** every assembly whose name matches `Anamnesis.Adapter.*.Contract` has exactly four dot-separated segments

---

### Requirement: Adapter implementation projects are named with a domain concept and technology segment
Every `Anamnesis.Adapter.<DomainConcept>.<Technology>` project name (non-Contract) SHALL have exactly the form `Anamnesis.Adapter.<DomainConcept>.<Technology>`, where `<DomainConcept>` is a PascalCase segment representing the domain concept (e.g. `MedicalData`, `Llm`) and `<Technology>` is a PascalCase segment representing the technology (e.g. `Nhs`, `Ollama`).

#### Scenario: Adapter implementation name follows the domain-technology pattern
- **WHEN** the architecture tests are run
- **THEN** every assembly whose name matches `Anamnesis.Adapter.*` and does not end in `.Contract` or `.Test` has exactly four dot-separated segments

---

### Requirement: UseCase projects follow the standard naming scheme
UseCase assemblies SHALL have the form `Anamnesis.UseCase.<Name>` and UseCase Contract assemblies `Anamnesis.UseCase.<Name>.Contract`.

#### Scenario: UseCase implementation name has three segments
- **WHEN** the architecture tests are run
- **THEN** every assembly whose name starts with `Anamnesis.UseCase.` and does not end in `.Contract` or `.Test` has exactly three dot-separated segments

#### Scenario: UseCase Contract name has four segments
- **WHEN** the architecture tests are run
- **THEN** every assembly whose name matches `Anamnesis.UseCase.*.Contract` has exactly four dot-separated segments

---

### Requirement: Test projects are named by appending `.Test` to their subject project
Every test project SHALL be named `<SubjectProjectName>.Test`.

#### Scenario: Test project name ends in `.Test`
- **WHEN** the architecture tests are run
- **THEN** every assembly that contains xUnit test classes has a name ending in `.Test`

---

### Requirement: Namespace matches assembly name
Every type in each production assembly SHALL reside in a namespace that starts with the assembly name.

#### Scenario: Root namespace matches assembly name
- **WHEN** the architecture tests are run
- **THEN** no type in any `Anamnesis.*` production assembly has a namespace that does not start with the assembly's name

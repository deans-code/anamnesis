---
name: architecture
description: Specifies how to implement software architecture in this repository, including project structure and layering. Use this skill as a reference for all implementation work to ensure consistency and maintainability across the codebase. Check this skill before starting any implementation task to align with the architectural guidelines defined here.
---

## Rule 1: Use a `src` Directory

ALWAYS place source code under a `src` directory at the repository root.

Exception: Only place files in the repository root when the specific technology requires it (e.g., a framework that mandates root-level config files).

---

## Rule 2: Apply Separation of Concerns to Every Feature

When implementing ANY feature, you MUST split code across exactly these four layers. Do not merge
layers.

| Layer | Responsibility | Contains |
|-------|---------------|----------|
| **Interface** | User interaction and display | Controllers, views, UI components, CLI commands |
| **Use Case** | Business logic | Application services, orchestration, workflow logic |
| **Domain** | Core concepts and rules | Entities, value objects, domain services, domain events |
| **Adapter** | External system integration | API clients, data mappers, repository implementations |

This applies to EVERY feature, without exception.

---

## Rule 3: Use .NET Projects for Each Layer

Organize code using separate .NET projects under `src/`. Each project follows a strict naming convention.

### Project naming convention

Replace the placeholders as follows:
- `{Repo}` — the repository name in PascalCase (e.g., `Anamnesis`)
- `{Interface}` — the UI technology in PascalCase (e.g., `Website`, `API`, `CLI`)
- `{UseCase}` — the use case name in PascalCase (e.g., `SymptomIntake`, `SessionSummary`)
- `{Adapter}` — the external service name in PascalCase (e.g., `Ollama`, `SqlServer`)

### Required projects per layer

**Interface layer**
- `{Repo}.Interface.{Interface}` — UI entry point and all interface-related files
- `{Repo}.Interface.{Interface}.Test` — Unit and integration tests for the interface layer

**Use Case layer**
- `{Repo}.UseCase.{UseCase}.Contract` — Request/response models and use case contracts
- `{Repo}.UseCase.{UseCase}` — Business logic and application services
- `{Repo}.UseCase.{UseCase}.Test` — Unit and integration tests for the use case

**Domain layer**
- `{Repo}.Domain.Contract` — Interfaces for domain services and repositories
- `{Repo}.Domain` — Entities, value objects, and domain services
- `{Repo}.Domain.Test` — Unit and integration tests for the domain

**Adapter layer**
- `{Repo}.Adapter.{Adapter}.Contract` — Interfaces for the adapter
- `{Repo}.Adapter.{Adapter}` — Implementation: API clients, data mappers, etc.
- `{Repo}.Adapter.{Adapter}.Test` — Unit and integration tests for the adapter

### Example (repository name: `Anamnesis`, use case: `SymptomIntake`, adapter: `Ollama`)

```
src/
  Anamnesis.Interface.Website/
  Anamnesis.Interface.Website.Test/
  Anamnesis.UseCase.SymptomIntake.Contract/
  Anamnesis.UseCase.SymptomIntake/
  Anamnesis.UseCase.SymptomIntake.Test/
  Anamnesis.Domain.Contract/
  Anamnesis.Domain/
  Anamnesis.Domain.Test/
  Anamnesis.Adapter.Ollama.Contract/
  Anamnesis.Adapter.Ollama/
  Anamnesis.Adapter.Ollama.Test/
```

---

## Rule 4: Define Interfaces in `.Contract` Projects

Any service, client, repository, or similar class created in the **Adapter**, **Use Case**, or **Domain** layers that is **consumed by another layer** MUST have a corresponding interface defined in the matching `.Contract` project.

**REQUIRED:**
- If you create `MyService` in `{Repo}.UseCase.{UseCase}` and it is used outside that project, define `IMyService` in `{Repo}.UseCase.{UseCase}.Contract`.
- If you create `MyClient` in `{Repo}.Adapter.{Adapter}` and it is used outside that project, define `IMyClient` in `{Repo}.Adapter.{Adapter}.Contract`.
- If you create `MyDomainService` in `{Repo}.Domain` and it is used outside that project, define `IMyDomainService` in `{Repo}.Domain.Contract`.
- The implementation class MUST implement its interface (e.g., `public class MyService : IMyService`).
- Consumers MUST depend on the interface, never the concrete class.
- Interfaces and their implementations MUST be registered with the DI container and injected via constructor injection wherever they are required.

**EXCEPTION:** If a class is only used internally within its own project and is never consumed by another layer, an interface is not required.

---

## Rule 5: Write Unit Tests for Every Service

Any class that has a corresponding interface in a `.Contract` project MUST have unit tests in the matching `.Test` project.

**REQUIRED:**
- If you create `MyService` in `{Repo}.UseCase.{UseCase}`, add tests in `{Repo}.UseCase.{UseCase}.Test`.
- If you create `MyClient` in `{Repo}.Adapter.{Adapter}`, add tests in `{Repo}.Adapter.{Adapter}.Test`.
- If you create `MyDomainService` in `{Repo}.Domain`, add tests in `{Repo}.Domain.Test`.
- Tests MUST be written against the interface (e.g., `IMyService`), not the concrete class directly.
- Tests MUST cover the public behaviour of each class, including success paths and expected failure/edge cases.
- Do NOT leave a `.Test` project empty when a corresponding implementation exists.

**EXCEPTION:** If a class has no interface (i.e., it is internal to its own project), unit tests are not required.

---

## Rule 6: Store All Settings in Configuration Files

NEVER hardcode software settings (API endpoints, model names, connection strings, feature flags, timeouts, etc.) in source code.

**REQUIRED:**
- Store all settings in `appsettings.json` (and environment-specific variants).
- Read settings at application startup via dependency injection.
- Use environment-specific files where needed: `appsettings.Development.json`, `appsettings.Staging.json`, `appsettings.Production.json`.

**REQUIRED for lower layers (Domain, Use Case, Adapter):**
- Do NOT read configuration files directly in lower layers.
- Settings MUST be passed down from the Interface layer or injected via the DI container.
- This keeps lower layers testable and free of infrastructure concerns.

## Rule 7: Implement Security as a Crosscutting Concern

Security is NOT a use case. Do NOT create a dedicated use case layer project for security.

Security is a crosscutting concern that MUST be implemented within each layer according to what the security code is addressing:

| Security concern | Where to implement |
|-----------------|-------------------|
| User input validation within forms | **Interface** (`{Repo}.Interface.{Interface}`) |
| Policy enforcement specific to a business function | **Use Case** (`{Repo}.UseCase.{UseCase}`) |
| Security concerns specific to an external system | **Adapter** (`{Repo}.Adapter.{Adapter}`) |

**REQUIRED:**
- Implement security at the layer responsible for the concern, not in a separate dedicated project.
- Do NOT centralise all security logic into one place if it belongs to different layers.


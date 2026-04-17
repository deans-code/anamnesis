---
applyTo: "**"
---

# Coding Instructions

These instructions apply to ALL files in this repository. Follow them exactly when generating, editing, or reviewing code.

---

## Rule 1: Use a `src` Directory

ALWAYS place source code under a `src` directory at the repository root.

Exception: Only place files in the repository root when the specific technology requires it (e.g., a framework that mandates root-level config files).

---

## Rule 2: Apply Separation of Concerns to Every Feature

When implementing ANY feature, you MUST split code across exactly these four layers. Do not merge layers.

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

## Rule 4: Store All Settings in Configuration Files

NEVER hardcode software settings (API endpoints, model names, connection strings, feature flags, timeouts, etc.) in source code.

**REQUIRED:**
- Store all settings in `appsettings.json` (and environment-specific variants).
- Read settings at application startup via dependency injection.
- Use environment-specific files where needed: `appsettings.Development.json`, `appsettings.Staging.json`, `appsettings.Production.json`.

**REQUIRED for lower layers (Domain, Use Case, Adapter):**
- Do NOT read configuration files directly in lower layers.
- Settings MUST be passed down from the Interface layer or injected via the DI container.
- This keeps lower layers testable and free of infrastructure concerns.
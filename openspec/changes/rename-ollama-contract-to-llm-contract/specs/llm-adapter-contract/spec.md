## ADDED Requirements

### Requirement: LLM adapter contract is defined independently of any specific LLM provider
The solution SHALL contain a project `Anamnesis.Adapter.Llm.Contract` that defines the exception surface for any LLM adapter: `LlmUnavailableException` and `RateLimitExceededException`. This project SHALL NOT reference any provider-specific type or name. `Anamnesis.Adapter.Ollama.Contract` SHALL NOT exist in the solution.

#### Scenario: LLM adapter contract project exists in the solution
- **WHEN** the solution file is inspected
- **THEN** `Anamnesis.Adapter.Llm.Contract` SHALL be registered as a project and `Anamnesis.Adapter.Ollama.Contract` SHALL NOT be registered

#### Scenario: Contract project contains no Ollama-specific identifiers
- **WHEN** the source files in `Anamnesis.Adapter.Llm.Contract` are inspected
- **THEN** no type name, namespace, or string SHALL contain "Ollama"

### Requirement: IOllamaClient is an internal seam within the Ollama adapter
`IOllamaClient` SHALL be defined as an internal interface inside `Anamnesis.Adapter.Ollama`. It SHALL NOT appear in any contract project. `Anamnesis.Adapter.Ollama` SHALL grant `InternalsVisibleTo` access to `Anamnesis.Adapter.Ollama.Test` so the test project can substitute `IOllamaClient`.

#### Scenario: IOllamaClient is not in any contract project
- **WHEN** the source files of `Anamnesis.Adapter.Llm.Contract` are inspected
- **THEN** `IOllamaClient` SHALL NOT be defined there

#### Scenario: Test project can mock IOllamaClient
- **WHEN** `Anamnesis.Adapter.Ollama.Test` uses NSubstitute to substitute `IOllamaClient`
- **THEN** the substitution SHALL succeed and all existing `OllamaConversationEngine` tests SHALL pass

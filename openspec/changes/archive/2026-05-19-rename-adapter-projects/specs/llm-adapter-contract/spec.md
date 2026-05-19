## MODIFIED Requirements

### Requirement: IOllamaClient is an internal seam within the Ollama adapter
`IOllamaClient` SHALL be defined as an internal interface inside `Anamnesis.Adapter.Llm.Ollama`. It SHALL NOT appear in any contract project. `Anamnesis.Adapter.Llm.Ollama` SHALL grant `InternalsVisibleTo` access to `Anamnesis.Adapter.Llm.Ollama.Test` so the test project can substitute `IOllamaClient`.

#### Scenario: IOllamaClient is not in any contract project
- **WHEN** the source files of `Anamnesis.Adapter.Llm.Contract` are inspected
- **THEN** `IOllamaClient` SHALL NOT be defined there

#### Scenario: Test project can mock IOllamaClient
- **WHEN** `Anamnesis.Adapter.Llm.Ollama.Test` uses NSubstitute to substitute `IOllamaClient`
- **THEN** the substitution SHALL succeed and all existing `OllamaConversationEngine` tests SHALL pass

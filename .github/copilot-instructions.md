# Custom Instructions

## src directory

Ensure that you use a `src` directory in the root of the repository for all source code.

This can be ignored only when the technology being used specifically requires artifacts to be in the root.

## Separation of Concerns

When implementing a feature, split the code into these basic layers:

 - Interface: This layer is responsible for handling user interactions and displaying information. It should contain the code for the user interface, such as controllers, views, and related files.

 - Use Case: This layer is responsible for implementing the business logic of the application. It should contain the code for the use cases, such as application services and related files.

 - Domain: This layer is responsible for representing the core concepts and rules of the application. It should contain the code for the domain entities, value objects, domain services, and related files.

 - Adapter: This layer is responsible for integrating with external services or systems. It should contain the code for the adapters, such as API clients, data mappers, and related files.

Do this for EVERY feature.

## Structure

Favour the use of .NET projects to separate concerns and organize code.

Ensure that each .NET project exists in a subdirectory of the `src` directory.

Use the following structure example:

`<RepositoryName>.Interface.<InterfaceName>`
  - This project contains the code for the interface layer of the application. It should include all related files for the specific interface.
  - Example `InterfaceName` values could be: Website, API, CLI, etc.

`<RepositoryName>.Interface.<InterfaceName>.Test`
  - This project contains the test code for the interface layer. It should include unit tests, integration tests, and any other tests related to the interface.

`<RepositoryName>.UseCase.Contract`
  - This project contains the code for the contracts of the use cases. It should include all the request and response models, as well as any other related files.

`<RepositoryName>.UseCase`
  - This project contains the code for the use cases of the application. It should include all the business logic and application services.

`<RepositoryName>.UseCase.Test`
  - This project contains the test code for the use cases. It should include unit tests, integration tests, and any other tests related to the use cases.

`<RepositoryName>.Domain.Contract`
  - This project contains the code for the contracts of the domain layer. It should include all the interfaces for the domain services, repositories, and any other related files.

`<RepositoryName>.Domain`
  - This project contains the code for the domain layer of the application. It should include all the domain entities, value objects, and domain services.

`<RepositoryName>.Domain.Test`
  - This project contains the test code for the domain layer. It should include unit tests, integration tests, and any other tests related to the domain.

`<RepositoryName>.Adapter.<ExternalServiceName>.Contract`
  - This project contains the code for the contracts of the adapter that integrates with an external service. It should include all the interfaces and related files for the adapter.

`<RepositoryName>.Adapter.<ExternalServiceName>` 
  - This project contains the code for the adapter that integrates with an external service. It should include all the necessary code to communicate with the external service, such as API clients, data mappers, and related files.

`<RepositoryName>.Adapter.<ExternalServiceName>.Test` 
  - This project contains the test code for the adapter that integrates with an external service. It should include unit tests, integration tests, and any other tests related to the adapter.

Where: 

- `<RepositoryName>` is the name of the repository, following PascalCase convention.
- `<ExternalServiceName>` is the name of the external service being integrated with, following PascalCase convention.

## Software settings

All software settings (e.g., API endpoints, model names, feature flags) MUST be stored in configuration files (e.g., `appsettings.json`) and SHOULD be read by the application at startup. These settings SHOULD NOT be hardcoded in the source code.

Use development, staging, and production configuration files as needed (e.g., `appsettings.Development.json`, `appsettings.Staging.json`, `appsettings.Production.json`) to manage environment-specific settings.

Where lower layers (e.g., domain, use case) require access to certain settings, these SHOULD be passed down from the interface layer or injected via dependency injection, rather than having lower layers read configuration files directly. This ensures separation of concerns and makes the code more testable.
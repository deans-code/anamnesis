# Custom Instructions

## src directory

Ensure that you use a `src` directory in the root of the repository for all source code.

This can be ignored only when the technology being used specifically requires artifacts to be in the root.

## Structure

Favour the use of .NET projects to separate concerns and organize code.

Ensure that each .NET project exists in a subdirectory of the `src` directory.

Use the following structure example:

`<RepositoryName>.Interface.Website`
 - This project contains the code for the website interface of the application. It should include all controllers, views, and related files.

`<RepositoryName>.Interface.Website.Test`
  - This project contains the test code for the website interface. It should include unit tests, integration tests, and any other tests related to the website.

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
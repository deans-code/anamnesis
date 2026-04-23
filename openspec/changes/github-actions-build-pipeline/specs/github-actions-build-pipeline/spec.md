## ADDED Requirements

### Requirement: CI workflow triggers on main branch activity
The system SHALL include a GitHub Actions workflow that is triggered automatically on any push to the `main` branch and on any pull request whose base branch is `main`.

#### Scenario: Push to main triggers build
- **WHEN** a commit is pushed directly to the `main` branch
- **THEN** the GitHub Actions workflow starts a new run

#### Scenario: Pull request targeting main triggers build
- **WHEN** a pull request is opened, synchronised, or reopened against `main`
- **THEN** the GitHub Actions workflow starts a new run

### Requirement: CI workflow restores NuGet dependencies
The workflow SHALL restore all NuGet packages for the solution before attempting to build.

#### Scenario: Dependencies restored before build
- **WHEN** the CI workflow runs
- **THEN** `dotnet restore` completes successfully before any build step executes

### Requirement: CI workflow builds the full solution
The workflow SHALL compile the entire solution (all projects) in release configuration without errors.

#### Scenario: Successful build
- **WHEN** all source files are valid and dependencies are restored
- **THEN** `dotnet build` exits with code 0 and produces compiled output for every project

#### Scenario: Build failure blocks the run
- **WHEN** any project fails to compile
- **THEN** the workflow run is marked as failed and subsequent steps do not execute

### Requirement: CI workflow runs all tests
The workflow SHALL execute all test projects in the solution and report results.

#### Scenario: All tests pass
- **WHEN** every test in every test project passes
- **THEN** the workflow run is marked as successful

#### Scenario: Test failure fails the run
- **WHEN** one or more tests fail
- **THEN** the workflow run is marked as failed with a non-zero exit code

### Requirement: CI workflow uses a compatible .NET SDK
The workflow SHALL configure the runner with the .NET SDK version required by the solution.

#### Scenario: Correct SDK version installed
- **WHEN** the workflow starts on the runner
- **THEN** the `actions/setup-dotnet` action installs the SDK version that matches the solution's target framework before any `dotnet` commands are executed

## Why

The project currently has no automated build verification on the `main` branch, meaning broken builds can go undetected until a developer manually runs the solution. Adding a CI pipeline ensures every push and pull request targeting `main` is validated automatically, providing fast feedback and protecting branch integrity.

## What Changes

- Add a GitHub Actions workflow file that triggers on pushes and pull requests to `main`
- The workflow will restore NuGet dependencies, build the entire solution, and run all test projects
- No existing application code, APIs, or configuration files will be modified

## Capabilities

### New Capabilities

- `github-actions-build-pipeline`: A CI workflow that restores, builds, and tests the .NET solution on every push or pull request targeting `main`

### Modified Capabilities

## Impact

- Adds `.github/workflows/build.yml` to the repository
- Requires a GitHub-hosted runner with the .NET SDK version matching the solution's target framework
- No changes to application source code, NuGet packages, or project files

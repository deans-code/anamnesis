## Context

The Anamnesis solution is a multi-project .NET application composed of several libraries and a Blazor WebAssembly/Server interface. It is hosted in a GitHub repository but currently has no CI pipeline. Developers build and test locally, meaning regressions on `main` are not caught automatically.

The change introduces a single GitHub Actions workflow file. No application source changes are required.

## Goals / Non-Goals

**Goals:**
- Automatically restore, build, and test the full solution on every push to `main` and every pull request targeting `main`
- Use the GitHub-hosted `ubuntu-latest` runner to keep costs at zero and setup simple
- Fail the workflow visibly if any project fails to build or any test fails

**Non-Goals:**
- Publishing or deploying artifacts (out of scope for a build-only pipeline)
- Code coverage reporting or quality gates
- Matrix builds across multiple .NET SDK versions or OS targets
- Docker image builds

## Decisions

### Use `ubuntu-latest` runner
GitHub-hosted Ubuntu runners are free for public repositories and have the .NET SDK pre-installed. Using Windows runners would require no extra setup but is slower and incurs more minutes on private repositories.  
**Decision**: `ubuntu-latest`.

### Trigger on `push` to `main` and `pull_request` targeting `main`
Triggering on `pull_request` catches regressions before merge. Triggering on `push` to `main` ensures any direct commits (e.g., merge commits) are also verified.  
**Decision**: Both triggers with `branches: [main]`.

### Single job — no parallelisation
The solution is small. Splitting restore/build/test into separate jobs adds orchestration complexity with no meaningful benefit.  
**Decision**: Single `build` job with sequential steps.

### Derive .NET SDK version from `global.json` or `dotnet-version` action input
If a `global.json` exists it will be respected automatically by `actions/setup-dotnet`. If not, the workflow pins the SDK version explicitly to match the projects' `<TargetFramework>`.  
**Decision**: Use `actions/setup-dotnet` with the version set to match the solution's target framework (net9.0 → `9.x`).

## Risks / Trade-offs

- [Pinned SDK version drifts from project target] → Update the `dotnet-version` input in the workflow whenever the solution upgrades its target framework.
- [Tests require external services (Ollama)] → Integration tests that call a live Ollama endpoint will fail on the runner. Mitigation: those tests should already be guarded by a condition or category; verify before merging.
- [Workflow permissions] → The default `GITHUB_TOKEN` with `contents: read` is sufficient for a build-only pipeline; no elevated permissions are needed.

## Migration Plan

1. Add `.github/workflows/build.yml` to the repository.
2. Push to a feature branch and open a PR against `main` — the workflow will run and validate itself.
3. Merge once green.
4. No rollback risk; removing the file reverts the change entirely.

## Open Questions

- Does the repository contain a `global.json` specifying an SDK version? If so, `actions/setup-dotnet` should reference it rather than a hardcoded version.

## 1. Repository Preparation

- [x] 1.1 Confirm the solution's target framework (check any `.csproj` for `<TargetFramework>`) and note the .NET SDK version to use in the workflow
- [x] 1.2 Check whether a `global.json` exists at the repository root; if so, record the SDK version it pins

## 2. Workflow File

- [x] 2.1 Create the directory `.github/workflows/` at the repository root
- [x] 2.2 Create `.github/workflows/build.yml` with the workflow name, trigger configuration (`push` and `pull_request` on `branches: [main]`), and a single `build` job running on `ubuntu-latest`
- [x] 2.3 Add the `actions/checkout@v4` step to the job
- [x] 2.4 Add the `actions/setup-dotnet` step configured with the .NET SDK version identified in task 1.1 (or referencing `global.json` if present)
- [x] 2.5 Add a `dotnet restore` step targeting the solution file
- [x] 2.6 Add a `dotnet build` step with `--no-restore` and `--configuration Release`
- [x] 2.7 Add a `dotnet test` step with `--no-build` and `--configuration Release`

## 3. Verification

- [ ] 3.1 Push the workflow file on a feature branch and open a pull request against `main`; confirm the workflow run starts automatically
- [ ] 3.2 Verify the run completes successfully with all steps green
- [ ] 3.3 Introduce a deliberate compile error on the branch, confirm the build step fails and the run is marked failed, then revert
- [ ] 3.4 Merge the pull request once the workflow is confirmed working

# Issue: ASI-09 — Supply Chain Integrity

**Status:** [ ] Not Addressed  
**Risk Level:** MEDIUM  
**Source:** `code-analysis/owasp-compliance/2026-04-21-14-30.md`

---

## Description

The application has no supply chain integrity mechanisms. Dependencies are managed via NuGet without integrity verification.

## Evidence

- `Anamnesis.Interface.Website.csproj` references NuGet packages without integrity manifests:
  ```xml
  <PackageReference Include="MudBlazor" Version="9.3.0" />
  ```

- No `INTEGRITY.json` or manifest files with SHA-256 hashes.
- No SBOM (Software Bill of Materials) generation.
- No dependency pinning verification.

## What Passing Looks Like

```xml
<!-- GOOD: Dependency pinning with upper bounds -->
<PackageReference Include="MudBlazor" Version=">= 9.3.0 < 10.0.0" />
```

## TODO: Steps to Fix

- [ ] Generate an SBOM for the project (e.g., using `dotnet-sbom` or `syft`)
- [ ] Implement dependency pinning with upper bounds (e.g., `>= 9.3.0 < 10.0.0`)
- [ ] Add integrity verification for all dependencies (e.g., `dotnet restore --ignore-failed-sources`)
- [ ] Consider using a private NuGet feed with signed packages
- [ ] Add a CI/CD step that validates the SBOM on every build
- [ ] Add a `DEPENDENCIES.md` file documenting all dependencies and their versions
- [ ] Add unit tests for dependency integrity verification
- [ ] Add a pre-commit hook that checks for unpinning of dependencies

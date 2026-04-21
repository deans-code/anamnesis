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
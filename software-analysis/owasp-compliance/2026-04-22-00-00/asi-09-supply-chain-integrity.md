# ASI-09: Supply Chain Integrity — No Dependency Integrity Verification

## Status

- [ ] Addressed

## Risk

The agent's plugins, tools, and dependencies have no integrity verification. There are no signed packages, no SBOM, no dependency pinning with upper bounds, and no integrity manifests.

## Finding

**Status:** FAIL — No integrity manifests or plugin signing

### Evidence

**File:** `src/Anamnesis.Interface.Website/Anamnesis.Interface.Website.csproj`

```xml
<ItemGroup>
  <PackageReference Include="MudBlazor" Version="9.3.0" />
</ItemGroup>
```

**File:** `src/Anamnesis.Adapter.Ollama/Anamnesis.Adapter.Ollama.csproj`

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Options" Version="10.0.0" />
</ItemGroup>
```

### Analysis

- No `INTEGRITY.json` or manifest files with SHA-256 hashes for dependencies
- No signature verification on plugin/tool installation
- Dependency pinning exists (specific versions) but no upper bound verification
- No SBOM (Software Bill of Materials) generation
- No integrity verification for the Ollama client library
- No verification that the MedGemma model has not been tampered with
- No dependency auditing process

### What Passing Looks Like

```json
// GOOD: INTEGRITY.json manifest
{
  "version": "1.0",
  "dependencies": [
    {
      "name": "MudBlazor",
      "version": "9.3.0",
      "sha256": "abc123...",
      "signature": "sig456..."
    },
    {
      "name": "Microsoft.Extensions.Options",
      "version": "10.0.0",
      "sha256": "def789...",
      "signature": "sig012..."
    }
  ]
}
```

## Recommendation

1. Generate `INTEGRITY.json` manifests for all dependencies with SHA-256 hashes
2. Implement signature verification on plugin/tool installation
3. Pin dependencies with both lower and upper version bounds
4. Generate an SBOM for the project
5. Implement dependency auditing in the build pipeline
6. Verify model integrity for the MedGemma model before use

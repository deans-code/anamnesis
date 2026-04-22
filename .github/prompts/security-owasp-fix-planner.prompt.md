---
description: Generate an OpenSpec plan for fixing identified OWASP compliance issues.
---

# Generate OWASP Compliance Fix Plan

Gather all identified compliance issues from Markdown files stored in the `/software-analysis/owasp-compliance/` directory.

Filter the issues to those not marked as addressed.

Use the `openspec-propose` skill to plan the remediation of the unaddressed OWASP compliance issues.

Once an issue is addressed, update the corresponding Markdown file to mark it as addressed and add a brief note on the fix implemented.
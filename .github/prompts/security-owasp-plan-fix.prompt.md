---
description: Plans OWASP compliance fixes.
---

Read all Markdown files stored as descendents of the `/code-analysis/owasp-compliance/` directory.

Create a directory if it does not already exist at the path `/code-analysis/owasp-fixes/`.

For each Markdown file, create a subdirectory in `/code-analysis/owasp-fixes/` with the same name as the Markdown file (without the `.md` extension).

From the Markdown file, extract all OWASP compliance issues and their corresponding code snippets.

For each compliance issue, create a new Markdown file in the corresponding subdirectory in `/code-analysis/owasp-fixes/` with the following format: `issue-{n}.md`, where `{n}` is a sequential number starting from 1.

The file must include at the top a checkbox of whether the issue has been addressed.

The file should include the descriptive content from the initial report, relating to the issue.

Do NOT create a to-do list for fixing the issue.
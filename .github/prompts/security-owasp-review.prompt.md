---
description: Generate an OWASP compliance report.
---

# OWASP Compliance Review

## Review

Use the `agent-owasp-compliance` skill to perform a comprehensive review of the solution.

Start at the root of the repository and review all code, configuration files, and documentation.

Ensure that all reported issues are focused on one specific aspect of OWASP compliance.

Ensure that all suggested fixes are focused on one specific aspect of OWASP compliance.

Ensure that all suggested fixes are provided in the appropriate programming language to match the code they intend to fix.

## Document

Create the `/software-analysis/owasp-compliance/` directory, if it does not exist.

Create a directory named with the current date and time in the format `yyyy-MM-dd-hh-mm` (e.g., `2024-06-01-14-30`) inside the `/software-analysis/owasp-compliance/` directory.

For each identified compliance issue, create a separate Markdown file in the newly created directory. Use a concise and descriptive title for each issue file.

The file must include a checkbox at the top, indicating whether the issue has been addressed.

The file should include the descriptive content from the initial report, relating to the issue.

## Plan

Do NOT create a to-do list for fixing the issue.
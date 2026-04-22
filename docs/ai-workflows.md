# AI Workflows

This document captures the workflows I have followed in developing this project, and is intended to provide insight into how I have approached the development process, the tools and technologies I have used, and the challenges I have encountered along the way.

## Creating the initial arhitecture

An initial goal of this software, aside from testing MedGemma capabilties, was to explore the use of locally running LLMs in combination with OpenSpec.

My goal was to use a capable model, such as Claude Sonnet 4.6, to create detailed OpenSpec specifications which could then be implemented by a more lightweight model, such as Qwen 3.6, running locally on Ollama.

The primary goal here would be to reduce the cost of inference.

Key findings relating to this workflow when creating the initial architecture were:
- Claude Sonnet 4.6 was able to create detailed specifications based on high level guidance, and the resulting specifications were effective in guiding the implementation of the software.
- Qwen 3.6 and Gemma 4 running locally on Ollama, and GPT-5 Mini and Haiku 4.5 running on GitHub Copilot all struggled to implement the specification while also following additional instructions specified in the custom instructions, skills and agents.
- For initial architecture creation, it is likely more effective to use a single capable model, such as Claude Sonnet 4.6, for both specification and implementation, to ensure the best possible guidance and output.

> [!NOTE]
> The key takeaway from this workflow is that the lightweight models tested did not consistently adhere to instructions specified in the custom instructions, skills and agents. These instructions primarily defined an architecture to implement. This could be limited to when the application is greenfield in nature, updates to an existing application where example code can be provided may be more effective, or it may be that the lightweight models tested are not suitable for this purpose regardless of the context. Further testing is needed.

## OWASP review and remediation

The workflow for reviewing and remediating OWASP related issues has been as follows:
1. Use a custom prompt `security-owasp-review` to guide the review of the software for OWASP compliance, using the `agent-owasp-compliance` skill. This work has been completed using Qwen 3.6 running locally on Ollama.
2. Review the output of the review and check for any false positives or irrelevant issues, and remove these from the report.
3. Use a custom prompt `security-owasp-fix-planner` to create detailed OpenSpec specifications for remediating the identified issues. This work has been completed using Claude Sonnet 4.6.
4. Implement the remediation specifications and verify that the identified issues have been resolved.
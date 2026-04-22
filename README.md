# Anamnesis

## :movie_camera: Background

This application was created to test the capabilities of Google DeepMind's [MedGemma](https://deepmind.google/models/gemma/medgemma/) models, specifically its 27B variant which supports complex text and multimodal medical knowledge and reasoning.

> [!WARNING]
> This project is a proof of concept and should not be used for any real medical purposes. It is intended for research and educational purposes only.

> [!WARNING]
> I believe that the use of AI in medical applications is a promising area of research, however, it is crucial to remember that AI models are not perfect and can make mistakes. It is important to use them with caution and always consult a medical professional for any health-related concerns.

Anamnesis, from [Wikipedia](https://en.wikipedia.org/wiki/Medical_history):

> The medical history, case history, or anamnesis (from Greek: ἀνά, aná, "open", and μνήσις, mnesis, "memory") of a patient is a set of information the physicians collect over medical interviews. It involves the patient, and eventually people close to them, so to collect reliable/objective information for managing the medical diagnosis and proposing efficient medical treatments.

The application is a simple web based chat interface which helps a user explore medical conditions or symptoms.

## :white_check_mark: Scope

 - [x] Simple web interface.
 - [x] Collect patient information.
 - [x] Propose questions to ask the patient.

## :telescope: Future Gazing

TBC

## :beetle: Known defects

No known defects.

## :crystal_ball: Use of AI

### AI-assisted development

[GitHub Copilot](https://github.com/features/copilot) was used to assist in the development of this software.

### Spec-driven development with OpenSpec

I am using [OpenSpec](https://openspec.dev/) for a specification-driven development approach, which means that I am writing specifications for the software before implementing features.

I am trialling the use of Claude Sonnet 4.6 for authoring the specifications and Qwen 3.6 (running locally via Ollama) for implementing the software based on the specifications.

I am using custom instructions, skills and agents to guide the implementation.

The choice of model for implementation may need to be dynamic based on the complexity of the individual specification, with more complex tasks being routed to a more capable model.

> [!NOTE]
> Initial testing suggests that when completing more complex tasks such as the creation of the initial architecture, Claude Sonnet 4.6 is more effective. Qwen 3.6 and Gemma 4 running locally on Ollama, and GPT-5 Mini and Haiku 4.5 running on GitHub Copilot all struggled to follow guidance specified in the custom instructions, skills and agents.

### Awsome GitHub Copilot

I am selecting useful add-ons from the [Awsome GitHub Copilot](https://awesome-copilot.github.com/) collection, which can be installed using the GitHub CLI.

### OWASP security review and remediation

I am using the [Agent Owasp Compliance](https://github.com/github/awesome-copilot/tree/main/skills/agent-owasp-compliance) skill from Awesome GitHub Copilot to perform a security review based on the [OWASP ASI Top 10](https://owasp.org/Top10/).

I am then using OpenSpec to author a detailed remediation specification based on the identified issues.

> [!NOTE]
> I am using using Qwen 3.6 (running locally via Ollama) for the OWASP review. The OpenSpec remediation specification is being created using Claude Sonnet 4.6. This is under test, initial indications are that Qwen 3.6 creates acceptable reviews but struggles to create detailed specifications based on the review.

## :rocket: Getting Started

### :computer: System Requirements

#### Software

##### GitHub CLI

For installing GitHub Copilot Skills, you will need the GitHub CLI:

https://github.com/cli/cli/blob/trunk/docs/install_windows.md

#### Hardware

A system capable of running Ollama is required.

Details of my personal system are below.

![APU](https://img.shields.io/badge/APU-AMD_Ryzen_AI_Max_395+-yellow "APU")

> [!NOTE]
> The hardware in use on my PC includes an Accelerated Processor Unit (APU) which combines CPU and GPU on a single chip. Recommendations for alternative hardware can be found [here](https://docs.ollama.com/gpu), performance will depend upon the models you choose to run (and other operational factors).

### :floppy_disk: System Configuration

#### Ollama 

Follow the [Ollama documentation](https://docs.ollama.com/) to install and configure Ollama on your system. 

You will need to pull the [MedGemma:27b](https://ollama.com/library/medgemma) to run the application.

### :wrench: Development Setup

Clone the repository.

Open in Visual Studio code.

Build the projects.

## :zap: Features

Simple chat interface for exploring medical conditions and symptoms.

The application is designed to help individuals understand potential medical conditions based on symptoms they input, and to suggest follow-up questions that a healthcare provider might ask to gather more information.

> [!WARNING]
> This software is designed for testing and educational purposes only. It is not intended for real medical use and should not be used as a substitute for professional medical advice, diagnosis, or treatment.

## :paperclip: Usage

Start the application.

Begin a conversation with the chatbot by entering symptoms or conditions you would like to explore.

The chatbot will ask follow-up questions to gather more information and provide insights based on the input.

You can end the session by clicking the "End Session" button.

A summary of your conversation will be displayed at the end.

## :wave: Contributing

This repository was created primarily for my own exploration of the technologies involved.

## :gift: License

I have selected an appropriate license using [this tool](https://choosealicense.com//).

This software is licensed under the [MIT](LICENSE) license.

## :book: Further reading

More detailed information can be found in the documentation:
* [AI workflows](docs/ai-workflows.md)
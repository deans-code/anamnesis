# Anamnesis

## :movie_camera: Background

This application was created to test the capabilities of Google DeepMind's [MedGemma](https://deepmind.google/models/gemma/medgemma/) models, specifically its 27B variant which supports complex text and multimodal medical knowledge and reasoning.

> [!WARNING]
> This project is a proof of concept and should not be used for any real medical purposes. It is intended for research and educational purposes only.

> [!WARNING]
> I believe that the use of AI in medical applications is a promising area of research, however, it is crucial to remember that AI models are not perfect and can make mistakes. It is important to use them with caution and always consult a medical professional for any health-related concerns.

Anamnesis, from [Wikipedia](https://en.wikipedia.org/wiki/Medical_history):

> The medical history, case history, or anamnesis (from Greek: ἀνά, aná, "open", and μνήσις, mnesis, "memory") of a patient is a set of information the physicians collect over medical interviews. It involves the patient, and eventually people close to them, so to collect reliable/objective information for managing the medical diagnosis and proposing efficient medical treatments.

The application is a simple web interface which helps a user explore medical conditions or symptoms.

## :white_check_mark: Scope

 - [ ] Simple web interface.
 - [ ] Collect patient information.
 - [ ] Propose questions to ask the patient.

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

Initial testing suggests that when completing more complex tasks such as the creation of the initial architecture, Claude Sonnet is more effective. Qwen 3.6 and Gemma 4 running locally on Ollama, and GPT-5 Mini and Haiku 4.5 running on GitHub Copilot all struggled to follow guidance specified in the custom instructions, skills and agents.

The choice of model for implementation may need to be dynamic based on the complexity of the individual specification, with more complex tasks being routed to a more capable model.

### Awsome GitHub Copilot

I am using selected add-ons from the [Awsome GitHub Copilot](https://awesome-copilot.github.com/) collection.

Installed using GitHub CLI.

## :rocket: Getting Started

### :computer: System Requirements

#### Software

##### GitHub CLI

For installing GitHub Copilot Skills, you will need the GitHub CLI:

```
https://github.com/cli/cli/blob/trunk/docs/install_windows.md
```

#### Visual Studio extensions

TBC

#### Hardware

TBC

### :floppy_disk: System Configuration

TBC

### :wrench: Development Setup

TBC

## :zap: Features

TBC

## :paperclip: Usage

TBC

## :question: Testing

TBC

## :raised_hands: Thanks

TBC

## :wave: Contributing

TBC

## :gift: License

TBC

## :book: Further reading

TBC

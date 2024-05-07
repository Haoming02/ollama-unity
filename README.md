<h2 align="center">W.I.P</h2>

<p align="center">
Just use <a href="https://github.com/awaescher/OllamaSharp">https://github.com/awaescher/OllamaSharp</a> instead...
</p>

# Ollama for Unity
A series of functions wrapping the [ollama APIs](https://github.com/ollama/ollama/blob/main/docs/api.md), mainly for use in Unity

## Prerequisite
The user machine needs to have a working ollama setup already

1. Download and Install [**ollama**](https://ollama.com/)
2. Pull a model of choice from the [model library](https://ollama.com/library)
    - Recommend `llama3` for general text conversation
        ```bash
        ollama pull llama3
        ```
    - Recommend `llava` for image captioning
        ```bash
        ollama run llava
        ```

## Features

- **List**
- **Generate**
- **Generate *(Stream)***

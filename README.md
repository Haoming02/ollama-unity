<h1 align="center">
<sub><img src="https://avatars.githubusercontent.com/u/151674099" width=64 height=64></sub>
<sup> for </sup>
<sub><img src="https://avatars.githubusercontent.com/u/426196" width=64 height=64></sub>
</h1>

<p align="center">
A series of C# functions wrapping the <a href="https://github.com/ollama/ollama/blob/main/docs/api.md">ollama APIs</a>, mainly for the <a href="https://unity.com/">UnityEngine</a>
</p>

## Prerequisite
Both the **developer** and the **user**'s systems need to have a working ollama setup already:

1. Download and Install [ollama](https://ollama.com/)
2. Pull a model of choice from the [Library](https://ollama.com/library)
    - Recommend `gemma3:4b` for general conversation
        ```bash
        ollama pull gemma3:4b
        ```
    - Recommend `gemma3:1b` for device with limited memory
        ```bash
        ollama pull gemma3:1b
        ```
    - Recommend `deepseek-r1:7b` if "reasoning" is needed
        ```bash
        ollama pull deepseek-r1:7b
        ```
    - Recommend `nomic-embed-text` for generating embeddings *(vectors)*
        ```bash
        ollama pull nomic-embed-text
        ```

In **Unity**, you need the `Newtonsoft.Json` package:

1. Unity Editor
2. Window
3. Package Manager
4. Add package by name
5. Name:
    ```
    com.unity.nuget.newtonsoft-json
    ```
6. Add

Then, simply download and install the `.unitypackage` from [Releases](https://github.com/Haoming02/ollama-unity/releases)

<br>

## Features
The following functions are available under the **Ollama** `static` class; all functions are [asynchronous](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/async)

- **List()**
    - Return an array of `Model`, representing all locally available models
    - The `Model` class follows the official [specs](https://github.com/ollama/ollama/blob/main/docs/api.md#list-local-models)

> [!TIP]
> You can use the `families` attribute to determine if a model is multimodal (see **[\#2608](https://github.com/ollama/ollama/issues/2608)**)

#### Generate

- **Generate()**
    - The most basic function that returns a response when given a model and a prompt
- **GenerateStream()**
    - The streaming variant that returns each word as soon as it's ready
    - Requires a `callback` to handle the chunks
- **GenerateJson()**
    - Return the response in the specified `class` / `struct` format

> [!IMPORTANT]
> You need to manually tell the model to use a JSON format in the prompt

#### Chat

- **Chat()**
    - Same as `Generate()`, but now with the memory of prior chat history, thus allowing you to further ask about previous conversations
    - Requires either `InitChat()` or `LoadChatHistory()` to be called first
    - **Example:**
        ```
                  "Tell me a joke" <<
        >> ...
                "Explain the joke" <<
        >> ...
        ```
- **ChatStream()**
    - Same as above
- **InitChat()**
    - Initialize / Reset the chat history
    - `historyLimit`: The number of messages to keep in memory
- **SaveChatHistory()**
    - Save the current chat history to the specified path
- **LoadChatHistory()**
    - Load the chat history from the specified path
    - Calls `InitChat()` automatically instead if the file does not exist

#### RAG

> **R**etrieval **A**ugmented **G**eneration

- **Ask()**
    - Ask a question based on given context
    - Requires both `InitRAG()` and `AppendData()` to be called first
- **InitRAG()**
    - Initialize the database
    - Requires a model to generate embeddings
        - Can use a different model from the one used in `Ask()`

> [!IMPORTANT]
> Not all models can generate embeddings

- **AppendData()**
    - Add a context *(**eg.** a document)* to retrieve from

<br>

## Demos
3 demo scenes showcasing various features are included:

- **Generate Demo**
    - `List()`
    - `Generate()`
    - `GenerateJson()`

- **Chat Demo**
    - `InitChat()`
    - `ChatStream()`

- **RAG Demo**
    - `InitRAG()`
    - `AppendData()`
    - `Ask()`

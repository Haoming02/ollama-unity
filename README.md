<h1 align="center">
<img src="https://avatars.githubusercontent.com/u/151674099" width=64 height=64>
<sup>for</sup>
<img src="https://avatars.githubusercontent.com/u/426196" width=64 height=64>
</h1>

<p align="center">
A series of C# functions wrapping the <a href="https://github.com/ollama/ollama/blob/main/docs/api.md">ollama APIs</a>, mainly for UnityEngine
</p>

## Prerequisite
The **user**'s system needs to have a working ollama setup already:

1. Download and Install [ollama](https://ollama.com/)
2. Pull a model of choice from the [Library](https://ollama.com/library)
    - Recommend `llama3.1` for general conversation
        ```bash
        ollama pull llama3.1
        ```
    - Recommend `gemma2:2b` for device with limited memory
        ```bash
        ollama pull gemma2:2b
        ```
    - Recommend `llava` for image captioning
        ```bash
        ollama pull llava
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

## Features
The following functions are avaliable under the **Ollama** class

> All functions are [asynchronous](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/async)

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
        >> Tell me a joke
        "..."
        >> Explain the joke
        "..."
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
        - Can use a regular LLM or a dedicated embedding model, such as `nomic-embed-text`
- **AppendData()**
    - Add a context *(**eg.** a document)* to retrieve from

> [!NOTE]
> How well the RAG performs is dependent on several factors...

## Demo
A demo scene containing 3 demo scripts showcasing various features is included:

- **Generate Demo**
    - `List()`
    - `Generate()`
    - `GenerateJson()`
    - `KeepAlive.unload_immediately`

- **Chat Demo**
    - `InitChat()`
    - `ChatStream()`

- **RAG Demo**
    - `InitRAG()`
    - `AppendData()`
    - `Ask()`

> [!NOTE]
> Recommended to not enable multiple demos at the same time...

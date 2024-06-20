# Ollama for Unity
A series of functions wrapping the [ollama APIs](https://github.com/ollama/ollama/blob/main/docs/api.md), mainly for use in Unity

## Prerequisite
The user's system needs to have a working ollama setup already:

1. Download and Install [**ollama**](https://ollama.com/)
2. Pull a model of choice from the [model library](https://ollama.com/library)
    - Recommend `llama3` for general text conversation
        ```bash
        ollama pull llama3
        ```
    - For coding-related inquiries, also try out `stable-code`
        ```bash
        ollama pull stable-code
        ```
    - Recommend `llava` for image captioning
        ```bash
        ollama pull llava:13b
        ```
    - Recommend `mxbai-embed-large` for embeddings
        ```bash
        ollama pull mxbai-embed-large
        ```

In Unity, you also need the **Newtonsoft.Json** package:

- Unity Editor -> **Window** -> **Package Manager** -> **`+`** -> **Add package by name** -> `com.unity.nuget.newtonsoft-json`

## Features
All functions below are [asynchronous](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/async). Simply call the functions under the `Ollama` class. Two demo scripts showcasing how to use each function are included.

- **List()**
    - Return an array of all available models that you can pass to other functions below
- **ListCategorized()**
    - Return two arrays, regular text models *(**eg.** `llama3`)* and multimodal models that can handle image inputs *(**eg.** `llava`)*, separately instead
- **Generate()**
    - The most basic function that returns the response when given a prompt
- **GenerateStream()**
    - The stream variant that passes each word as soon as it's ready *(like ChatGPT)*. Requires a callback to handle the texts.
- **GenerateWithImage()**
    - Generate a response based on an image. Requires a multimodal model *(**eg.** `llava`)*. Supports `UnityEngine`'s `Texture2D` directly.
- **GenerateWithImageStream()**
    - Same as above
- **GenerateJson()**
    - Give a `class` / `struct` format for the model to reply in
    - **Important:** You need to manually tell the model to reply in JSON format in the prompt as well
- **Chat()**
    - Same as `Generate()`, but with the memory of prior chat history, thus allowing you to further ask about previous conversations. Requires either `InitChat()` or `LoadChatHistory()` to be called first.
    - **Example:**
        ```
        >> Tell me a joke
        "..."
        >> Explain the joke
        "..."
        ```
- **ChatStream()**
    - Same as above
- **ChatWithImage()**
    - Same as above
- **ChatWithImageStream()**
    - Same as above
- **InitChat()**
    - Initialize / Reset the chat history
    - `historyLimit`: The number of messages to keep in memory *(defaults to `16`)*
- **SaveChatHistory()**
    - Save the current chat history to the specified path *(defaults to `Application.persistentDataPath`)*
- **LoadChatHistory()**
    - Load the chat history from the specified path *(defaults to `Application.persistentDataPath`)*. Will simply initialize if the file does not exist.

> **Note:** All model-related functions *(**eg.** `create`, `copy`, `pull`, etc.)* will **not** be implemented

## **R**etrieval **A**ugmented **G**eneration
<p align="right"><sup><i>experimental</i></sup></p>

> Based on [ChromaDB](https://www.trychroma.com/)

- **Ask()**
    - Ask a question based on the given context. Requires the following to be called first.
- **InitRAG()**
    - Start the ChromaDB server and initialize the database
    - `pythonPath`: The path to the Python folder. See [Prerequisite](#prerequisite).
    - `authToken`: ChromaDB allows you to set a password to prevent other users from accessing the database.
- **AppendData()**
    - Give the context *(**eg.** a document)* to perform RAG on

#### Prerequisite
This repo comes with an **Editor** script that helps you install the necessary self-contained **Python** environment for running the ChromaDB.

Simply go to the Editor, click **Ollama** in the toolbar, then click **Obtain Python**. Specify a path to store the environment, then click the 2 install buttons.

Afterwards, when you call `InitRAG`, remember to pass in the path to the **python** folder.

#### W.I.P
- **AskChat()**
    - Ask a question based on the given context as well as chat history

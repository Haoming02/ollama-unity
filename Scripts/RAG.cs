using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using UnityEngine;

public static partial class Ollama
{
    private static Message CONTEXT;

    /// <summary> Setup the context to perform RAG on </summary>
    public static void InitRAG(TextAsset data) => InitRAG(data.text);

    /// <summary> Setup the context to perform RAG on </summary>
    public static void InitRAG(string data)
    {
        CONTEXT = new Message("system",
            "You are an assistant for question-answering tasks. " +
            "Use only the following provided context to answer the question. " +
            "If you don't know the answer, simply just say that you don't know. \n" +
            "Context: \n" + data
        );
    }

    /// <summary> Ask a question for the given context using a provided model </summary>
    public static async Task<string> Ask(string prompt, string model = "llama3")
    {
        var request = new Request.Chat(model, new Message[] { CONTEXT, new Message("user", prompt) }, false);
        string payload = JsonConvert.SerializeObject(request);
        var response = await PostRequest<Response.Chat>(payload, Endpoints.CHAT);
        return response.message.content;
    }

    /// <summary> Stream a response for a question based on the given context using a provided model </summary>
    public static async Task AskStream(string prompt, Action<string> onTextReceived, string model = "llama3")
    {
        var request = new Request.Chat(model, new Message[] { CONTEXT, new Message("user", prompt) }, true);
        string payload = JsonConvert.SerializeObject(request);

        await PostRequestStream(payload, Endpoints.CHAT, (Response.Chat response) =>
        {
            onTextReceived?.Invoke(response.message.content);
        });

        OnStreamFinished?.Invoke();
    }
}

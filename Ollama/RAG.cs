using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static partial class Ollama
{
    private const string DB_NAME = "RAG_DB";
    private static string COLLECTION_ID;

    private static Dictionary<string, string> DB;

    /// <summary> Initialize the database to perform RAG on </summary>
    public static async Task InitRAG(string pythonPath, string authToken)
    {
        await ChromaDB.StartServer(pythonPath, authToken);

        int collectionCount = await ChromaDB.Count_Collections();
        DB = new Dictionary<string, string>();

        if (collectionCount == 0)
            COLLECTION_ID = await ChromaDB.Create_Collections(DB_NAME);
        else
            COLLECTION_ID = await ChromaDB.Collections(DB_NAME);
    }

    private const int CONTEXT_LIMIT = 4096;

    /// <summary> Add context into the database </summary>
    public static async Task AppendData(TextAsset data) => await AppendData(data.text);

    /// <summary> Add context into the database </summary>
    public static async Task AppendData(string data)
    {
        int len = data.Length;
        int chunk_count = Mathf.CeilToInt((float)len / CONTEXT_LIMIT);
        int context_length = len / chunk_count;

        for (int i = 0; i < chunk_count; i++)
        {
            string chunk;

            int from = (i == 0) ? 0 :
                data.Substring((i - 1) * context_length, context_length).LastIndexOf('.') + (i - 1) * context_length + 1;

            if (i == chunk_count - 1)
                chunk = data.Substring(from);
            else
            {
                int to = data.Substring((i + 1) * context_length, Mathf.Min(context_length, len - (i + 1) * context_length)).IndexOf('.') + (i + 1) * context_length + 1;
                chunk = data.Substring(from, to - from);
            }

            string hash = IO.Hash(chunk);
            if (await ChromaDB.Get(COLLECTION_ID, hash) == null)
            {
                float[] vector = await Embeddings(chunk);
                await ChromaDB.Add(COLLECTION_ID, hash, vector);
            }

            DB.Add(hash, chunk);
        }
    }

    private static async Task<Message> Analyze(string prompt)
    {
        var vector = await Embeddings(prompt);

        var id = await ChromaDB.Query(COLLECTION_ID, vector);

        return Context2Message(DB[id]);
    }

    /// <summary> Ask a question for the given context using a provided model </summary>
    public static async Task<string> Ask(string prompt, string model = "llama3")
    {
        var system = await Analyze(prompt);
        var request = new Request.Chat(model, new Message[] { system, new Message("user", prompt) }, false);
        string payload = JsonConvert.SerializeObject(request);
        var response = await PostRequest<Response.Chat>(payload, Endpoints.CHAT);
        return response.message.content;
    }

    /// <summary> Stream a response for a question based on the given context using a provided model </summary>
    public static async Task AskStream(string prompt, Action<string> onTextReceived, string model = "llama3")
    {
        var system = await Task.Run(async () => await Analyze(prompt));
        var request = new Request.Chat(model, new Message[] { system, new Message("user", $"Question:\n{prompt}") }, true);
        string payload = JsonConvert.SerializeObject(request);

        await PostRequestStream(payload, Endpoints.CHAT, (Response.Chat response) =>
        {
            onTextReceived?.Invoke(response.message.content);
        });

        OnStreamFinished?.Invoke();
    }

    private static Message Context2Message(string context)
    {
        return new Message("system",
            "You are an assistant for question-answering tasks. " +
            "Use only the following piece of context to answer the question. " +
            "If no answer can be found within the context, simply say so. Do not answer outside of context. \n" +
            "Context: \n" + context
        );
    }
}

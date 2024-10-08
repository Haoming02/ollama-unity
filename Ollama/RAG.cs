using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static partial class Ollama
{
    private const int CONTEXT_LIMIT = 1024;

    private static List<float[]> VectorsDatabase;
    private static List<string> ContextDatabase;

    private static class EmbedConfig
    {
        public static string model;
        public static KeepAlive keep_alive;
    }

    /// <summary>
    /// Initialize the database to store the vectors
    /// </summary>
    /// <param name="embeddingModel">
    /// Ollama Model Syntax (<b>eg.</b> llama3.1) <br/>
    /// <b>Note:</b> Not all models can generate Embeddings
    /// </param>
    /// <param name="keep_alive">
    /// The behavior to keep the embedding model loaded in memory
    /// </param>
    public static void InitRAG(string embeddingModel, KeepAlive keep_alive = KeepAlive.five_minute)
    {
        VectorsDatabase = new List<float[]>();
        ContextDatabase = new List<string>();
        EmbedConfig.model = embeddingModel;
        EmbedConfig.keep_alive = keep_alive;
    }

    /// <summary>
    /// Add context into the database and calculate vector for it
    /// </summary>
    /// <param name="data">Ensure longer texts contains newline in order to be split</param>
    public static async Task AppendData(string data)
    {
        int len = data.Length;
        if (len <= CONTEXT_LIMIT)
        {
            float[] vector = await Embeddings(EmbedConfig.model, data, EmbedConfig.keep_alive);
            VectorsDatabase.Add(vector);
            ContextDatabase.Add(data);
            return;
        }

        int chunkCount = Mathf.CeilToInt((float)len / CONTEXT_LIMIT);
        int contextLength = len / chunkCount;

        string[] lines = data.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        int lineCount = lines.Length;
        int i = 0;

        StringBuilder chunk = new StringBuilder();

        while (i < lineCount)
        {
            chunk.Clear();

            while (chunk.Length < contextLength && i < lineCount)
            {
                string line = lines[i].Trim();
                if (line.Length > contextLength && chunk.Length > contextLength / 2)
                    break;

                chunk.AppendLine(line);
                i++;
            }

            string paragraph = chunk.ToString();

            float[] vector = await Embeddings(EmbedConfig.model, paragraph, EmbedConfig.keep_alive);
            VectorsDatabase.Add(vector);
            ContextDatabase.Add(paragraph);
        }
    }

    private static async Task<Message> Analyze(string prompt)
    {
        var vector = await Embeddings(EmbedConfig.model, prompt, EmbedConfig.keep_alive);
        int l = ContextDatabase.Count;

        if (l == 1)
            return Context2Message(ContextDatabase[0]);
        else if (l == 0)
        {
            Debug.LogError("Database is Empty...");
            return null;
        }

        float max = -1.0f;
        int index = -1;

        for (int i = 0; i < l; i++)
        {
            float score = CosineSimilarity(vector, VectorsDatabase[i]);
#if UNITY_EDITOR
            Debug.Log($"[{score}]: {ContextDatabase[i].Trim().Substring(0, 128)}");
#endif
            if (score > max)
            {
                max = score;
                index = i;
            }
        }

        return Context2Message(ContextDatabase[index]);
    }

    /// <summary>
    /// Ask a question based on the given context
    /// </summary>
    /// <param name="model">Ollama Model Syntax (<b>eg.</b> llama3.1)</param>
    /// <param name="keep_alive">The behavior to keep the model loaded in memory</param>
    /// <returns>response string from the LLM</returns>
    public static async Task<string> Ask(string model, string prompt, KeepAlive keep_alive = KeepAlive.five_minute)
    {
        var system = await Analyze(prompt);
        var request = new Request.Chat(model, new Message[] { system, new Message("user", $"Question:\n{prompt}") }, false, keep_alive);
        string payload = JsonConvert.SerializeObject(request);
        var response = await PostRequest<Response.Chat>(payload, Endpoints.CHAT);
        return response.message.content;
    }

    /// <summary>
    /// Stream an answer based on the given context
    /// </summary>
    /// <param name="onTextReceived">The callback to handle the streaming chunks</param>
    /// <param name="model">Ollama Model Syntax (<b>eg.</b> llama3.1)</param>
    /// <param name="keep_alive">The behavior to keep the model loaded in memory</param>
    public static async Task AskStream(Action<string> onTextReceived, string model, string prompt, KeepAlive keep_alive = KeepAlive.five_minute)
    {
        var system = await Analyze(prompt);
        var request = new Request.Chat(model, new Message[] { system, new Message("user", $"Question:\n{prompt}") }, true, keep_alive);
        string payload = JsonConvert.SerializeObject(request);

        await PostRequestStream(payload, Endpoints.CHAT, (Response.Chat response) =>
        {
            if (!response.done)
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
            $"Context:\n```\n{context}\n```"
        );
    }

    public static void DebugContext()
    {
        foreach (var text in ContextDatabase)
            Debug.Log($"[{text.Length}]: {text.Trim()}");
    }
}

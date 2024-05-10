using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static partial class Ollama
{
    private static DB DATABASE;

    /// <summary> Initialize the database to perform RAG on </summary>
    public static void InitRAG() => DATABASE = new DB();

    private const int CONTEXT_LIMIT = 4096;

    /// <summary> Add context into the database </summary>
    public static async Task AppendData(TextAsset data) => await AppendData(data.text);

    /// <summary> Add context into the database </summary>
    public static async Task AppendData(string data)
    {
        int l = data.Length;
        if (l <= CONTEXT_LIMIT)
        {
            var vector = await Embeddings(data);
            DATABASE.Vectors.Add(vector);
            DATABASE.Contents.Add(data);
            DATABASE.Count++;
        }
        else
        {
            int chunk_count = Mathf.CeilToInt((float)l / CONTEXT_LIMIT);
            for (int i = 0; i < chunk_count; i++)
            {
                string chunk;

                int from = (i == 0) ? 0 :
                    data.Substring((i - 1) * CONTEXT_LIMIT, CONTEXT_LIMIT).LastIndexOf('.') + (i - 1) * CONTEXT_LIMIT + 1;

                if (i < chunk_count - 1)
                {
                    int to = data.Substring((i + 1) * CONTEXT_LIMIT, Mathf.Min(CONTEXT_LIMIT, l - (i + 1) * CONTEXT_LIMIT)).IndexOf('.') + (i + 1) * CONTEXT_LIMIT + 1;
                    chunk = data.Substring(from, to - from);
                }
                else
                    chunk = data.Substring(from);

                var vector = await Embeddings(chunk);
                DATABASE.Vectors.Add(vector);
                DATABASE.Contents.Add(chunk);
                DATABASE.Count++;
            }
        }
    }

    private static async Task<Message> Analyze(string prompt)
    {
        if (DATABASE.Count == 0)
            throw new NullReferenceException("Database is Empty...");

        if (DATABASE.Count == 1)
            return Context2Message(DATABASE.Contents[0]);

        var vector = await Embeddings(prompt);

        float best_match = -1.0f;
        int index = -1;

        for (int i = 0; i < DATABASE.Count; i++)
        {
            float similarity = CosineSimilarity(vector, DATABASE.Vectors[i]);
#if UNITY_EDITOR
            Debug.Log($"[{similarity.ToString("0.00")}]: {DATABASE.Contents[i].Substring(0, DATABASE.Contents[i].IndexOf(".") + 1)}");
#endif
            if (similarity > best_match)
            {
                best_match = similarity;
                index = i;
            }
        }

        return Context2Message(DATABASE.Contents[index]);
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
        var system = await Analyze(prompt);
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

    private class DB
    {
        public List<float[]> Vectors;
        public List<string> Contents;
        public int Count;

        public DB()
        {
            Vectors = new List<float[]>();
            Contents = new List<string>();
            Count = 0;
        }
    }

    //private static string[] SplitChunks(string input, int min_threshold = 1024, int max_threshold = 2048)
    //{
    //    if (input.Trim().Length < max_threshold)
    //        return new string[] { input };

    //    List<string> chunks = new List<string>();
    //    StringBuilder stringBuilder = new StringBuilder();
    //    int lenth = 0;

    //    string[] temp = input.Split('\n');

    //    foreach (var part in temp)
    //    {
    //        if (part.Trim().Length == 0)
    //            continue;

    //        if (lenth + part.Length > max_threshold)
    //        {
    //            chunks.Add(stringBuilder.ToString());
    //            stringBuilder.Clear();

    //            lenth = part.Length;
    //            stringBuilder.AppendLine(part);
    //        }
    //        else
    //        {
    //            lenth += part.Length;
    //            stringBuilder.AppendLine(part);

    //            if (lenth > min_threshold)
    //            {
    //                chunks.Add(stringBuilder.ToString());
    //                stringBuilder.Clear();
    //                lenth = 0;
    //            }
    //        }
    //    }

    //    return chunks.ToArray();
    //}
}

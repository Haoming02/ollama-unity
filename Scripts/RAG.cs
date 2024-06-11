using DBreeze;
using DBreeze.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

// ==================================== Reference ========================================= //
// https://github.com/hhblaze/DBreeze/blob/master/TesterNet6/TextCorpus/ITGiantLogotypes.cs //
// ======================================================================================== //

public static partial class Ollama
{
    private const string EMBEDDING = "vector";
    private const string DOCUMENT = "string";

    private static string DEFAULT_PATH => Path.Combine(Application.persistentDataPath, "RAG_DB");
    private static DBreezeEngine DATABASE;

    private static bool isInit = false;

    /// <summary> Initialize the database to perform RAG on </summary>
    public static void InitRAG(string filepath = null)
    {
        if (isInit)
            return;

        if (filepath == null)
            filepath = DEFAULT_PATH;

        DATABASE = new DBreezeEngine(filepath);
        Application.quitting += () => { DATABASE.Dispose(); };

        isInit = true;
    }

    /// <summary> Clear out all contexts in the database </summary>
    public static void ClearContext()
    {
        if (!isInit)
            return;

        if (DATABASE.Scheme.IfUserTableExists(EMBEDDING))
            DATABASE.Scheme.DeleteTable(EMBEDDING);
        if (DATABASE.Scheme.IfUserTableExists(DOCUMENT))
            DATABASE.Scheme.DeleteTable(DOCUMENT);
    }

    private const int CONTEXT_LIMIT = 4096;

    /// <summary> Add context into the database </summary>
    public static async Task AppendData(TextAsset data) => await AppendData(data.text);

    /// <summary> Add context into the database </summary>
    public static async Task AppendData(string data)
    {
        if (!isInit)
            return;

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

            await appendData(chunk);
        }
    }

    private static byte[] KEY_DocumentCount() => 1.ToIndex();
    private static byte[] KEY_DocumentID(int id) => 2.ToIndex(id);

    private static async Task appendData(string data)
    {
        using (var tran = DATABASE.GetTransaction())
        {
            tran.SynchronizeTables(EMBEDDING, DOCUMENT);

            int count = tran.Select<byte[], int>(DOCUMENT, KEY_DocumentCount()).Value;
            count++;

            tran.Insert<byte[], string>(DOCUMENT, KEY_DocumentID(count), data);

            double[] embedding = await Embeddings(data);
            Dictionary<byte[], double[]> vectors = new() {
                { KEY_DocumentID(count), embedding }
            };

            tran.VectorsInsert(EMBEDDING, vectors, false);

            tran.Insert<byte[], int>(DOCUMENT, KEY_DocumentCount(), count);

            tran.Commit();
        }
    }

    private static async Task<Message> Analyze(string prompt)
    {
        using (var tran = DATABASE.GetTransaction())
        {
            tran.ValuesLazyLoadingIsOn = false;

            if (tran.Count(DOCUMENT) == 0)
                throw new NullReferenceException("Database is Empty...");

            double[] embedding = await Embeddings(prompt);

#if UNITY_EDITOR
            var similar = tran.VectorsSearchSimilar(EMBEDDING, embedding, -1).ToArray();

            foreach (var vector in similar)
                Debug.Log(tran.Select<byte[], string>(DOCUMENT, vector).Value);

            var result = tran.Select<byte[], string>(DOCUMENT, similar.First()).Value;
#else
            var similar = tran.VectorsSearchSimilar(EMBEDDING, embedding, 1).First();
            var result = tran.Select<byte[], string>(DOCUMENT, similar).Value;
#endif

            return Context2Message(result);
        }
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
}

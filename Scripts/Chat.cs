using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static partial class Ollama
{
    private static Queue<Message> ChatHistory;
    private static int HistoryLimit = -1;

    /// <summary> Start a brand new chat </summary>
    public static void InitChat(int historyLimit = 16)
    {
        ChatHistory = new Queue<Message>();
        HistoryLimit = historyLimit;
    }

    /// <summary> Save the current Chat History to the specified path </summary>
    public static void SaveChatHistory(string fileName = null)
    {
        if (fileName == null)
            fileName = Path.Combine(Application.persistentDataPath, "chat.dat");

        using var stream = File.Open(fileName, FileMode.Create);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, false);

        var data = JsonConvert.SerializeObject(ChatHistory);
        writer.Write(IO.Encrypt(data));
        Debug.Log($"Chat History saved to \"{fileName}\"");
    }

    /// <summary> Load a Chat History from the specified path </summary>
    public static void LoadChatHistory(string fileName = null, int historyLimit = 16)
    {
        if (fileName == null)
            fileName = Path.Combine(Application.persistentDataPath, "chat.dat");

        if (File.Exists(fileName))
        {
            using var stream = File.Open(fileName, FileMode.Open);
            using var reader = new BinaryReader(stream, Encoding.UTF8, false);

            ChatHistory = JsonConvert.DeserializeObject<Queue<Message>>(IO.Decrypt(reader.ReadString()));
            HistoryLimit = historyLimit;
            Debug.Log($"Chat History loaded from \"{fileName}\"");
        }
        else
        {
            InitChat(historyLimit);
            Debug.LogWarning($"Chat History \"{fileName}\" does not exist!");
        }
    }

    /// <summary> Generate a response for a given prompt using a provided model with history </summary>
    public static async Task<string> Chat(string prompt, string system = null, string model = "llama3")
    {
        if (system != null)
            ChatHistory.Enqueue(new Message("system", system));

        ChatHistory.Enqueue(new Message("user", prompt));

        var request = new Request.Chat(model, ChatHistory.ToArray(), false);
        string payload = JsonConvert.SerializeObject(request);
        var response = await PostRequest<Response.Chat>(payload, Endpoints.CHAT);

        ChatHistory.Enqueue(response.message);

        while (ChatHistory.Count > HistoryLimit)
            ChatHistory.Dequeue();

        return response.message.content;
    }

    /// <summary> Stream a response for a given prompt using a provided model with history </summary>
    public static async Task ChatStream(string prompt, Action<string> onTextReceived, string system = null, string model = "llama3")
    {
        if (system != null)
            ChatHistory.Enqueue(new Message("system", system));

        ChatHistory.Enqueue(new Message("user", prompt));

        var request = new Request.Chat(model, ChatHistory.ToArray(), true);
        string payload = JsonConvert.SerializeObject(request);

        StringBuilder reply = new StringBuilder();

        await PostRequestStream(payload, Endpoints.CHAT, (Response.Chat response) =>
        {
            onTextReceived?.Invoke(response.message.content);
            reply.Append(response.message.content);
        });

        ChatHistory.Enqueue(new Message("assistant", reply.ToString()));

        while (ChatHistory.Count > HistoryLimit)
            ChatHistory.Dequeue();
    }


    /// <summary> Generate a response for a given prompt and image using a provided model with history </summary>
    public static async Task<string> ChatWithImage(string prompt, Texture2D image, string system = null, string model = "llava")
    {
        if (system != null)
            ChatHistory.Enqueue(new Message("system", system));

        ChatHistory.Enqueue(new Message("user", prompt, new string[] { Convert.ToBase64String(image.EncodeToJPG()) }));

        var request = new Request.Chat(model, ChatHistory.ToArray(), false);
        string payload = JsonConvert.SerializeObject(request);
        var response = await PostRequest<Response.Chat>(payload, Endpoints.CHAT);

        ChatHistory.Enqueue(response.message);

        while (ChatHistory.Count > HistoryLimit)
            ChatHistory.Dequeue();

        return response.message.content;
    }

    /// <summary> Stream a response for a given prompt and image using a provided model with history </summary>
    public static async Task ChatWithImageStream(string prompt, Texture2D image, Action<string> onTextReceived, string system = null, string model = "llava")
    {
        if (system != null)
            ChatHistory.Enqueue(new Message("system", system));

        ChatHistory.Enqueue(new Message("user", prompt, new string[] { Convert.ToBase64String(image.EncodeToJPG()) }));

        var request = new Request.Chat(model, ChatHistory.ToArray(), true);
        string payload = JsonConvert.SerializeObject(request);

        StringBuilder reply = new StringBuilder();

        await PostRequestStream(payload, Endpoints.CHAT, (Response.Chat response) =>
        {
            onTextReceived?.Invoke(response.message.content);
            reply.Append(response.message.content);
        });

        ChatHistory.Enqueue(new Message("assistant", reply.ToString()));

        while (ChatHistory.Count > HistoryLimit)
            ChatHistory.Dequeue();
    }
}

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
    private static Message systemPrompt = null;
    private static int HistoryLimit = -1;

    /// <summary>
    /// Start a brand new chat
    /// </summary>
    /// <param name="historyLimit">How many messages to keep in memory <i>(includes both query and reply)</i></param>
    public static void InitChat(int historyLimit = 8)
    {
        ChatHistory = new Queue<Message>();
        HistoryLimit = historyLimit;
    }

    /// <summary>
    /// Save the current Chat History to the specified path
    /// </summary>
    /// <param name="fileName">If empty, defaults to <b>Application.persistentDataPath</b></param>
    public static void SaveChatHistory(string fileName = null)
    {
        if (string.IsNullOrEmpty(fileName))
            fileName = Path.Combine(Application.persistentDataPath, "chat.dat");

        using var stream = File.Open(fileName, FileMode.Create);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, false);

        var data = JsonConvert.SerializeObject(ChatHistory);
        writer.Write(IO.Encrypt(data));
        Debug.Log($"Chat History saved to \"{fileName}\"");
    }

    /// <summary>
    /// Load a Chat History from the specified path
    /// </summary>
    /// <param name="historyLimit">How many messages to keep in memory <i>(includes both query and reply)</i></param>
    public static void LoadChatHistory(string fileName = null, int historyLimit = 8)
    {
        if (string.IsNullOrEmpty(fileName))
            fileName = Path.Combine(Application.persistentDataPath, "chat.dat");

        if (!File.Exists(fileName))
        {
            InitChat(historyLimit);
            Debug.LogWarning($"Chat History \"{fileName}\" does not exist!");
            return;
        }

        using var stream = File.Open(fileName, FileMode.Open);
        using var reader = new BinaryReader(stream, Encoding.UTF8, false);

        ChatHistory = JsonConvert.DeserializeObject<Queue<Message>>(IO.Decrypt(reader.ReadString()));
        HistoryLimit = historyLimit;
        Debug.Log($"Chat History loaded from \"{fileName}\"");
    }

    /// <summary>
    /// Generate a response from prompt with chat context/history
    /// </summary>
    /// <param name="model">Ollama Model Syntax (<b>eg.</b> llama3.1)</param>
    /// <param name="image">A multimodal model is required to handle images (<b>eg.</b> llava)</param>
    /// <param name="keep_alive">The behavior to keep the model loaded in memory</param>
    /// <returns>response string from the LLM</returns>
    public static async Task<string> Chat(string model, string prompt, Texture2D image = null, KeepAlive keep_alive = KeepAlive.five_minute)
    {
        ChatHistory.Enqueue(new Message("user", prompt, Texture2Base64(image)));
        Message[] messages;

        if (systemPrompt == null)
            messages = ChatHistory.ToArray();
        else
        {
            messages = new Message[ChatHistory.Count + 1];
            messages[0] = systemPrompt;
            ChatHistory.CopyTo(messages, 1);
        }

        var request = new Request.Chat(model, messages, false, keep_alive);
        string payload = JsonConvert.SerializeObject(request);
        var response = await PostRequest<Response.Chat>(payload, Endpoints.CHAT);

        ChatHistory.Enqueue(response.message);
        while (ChatHistory.Count > HistoryLimit)
            ChatHistory.Dequeue();

        return response.message.content;
    }

    /// <summary>
    /// Stream a response from prompt with chat context/history
    /// </summary>
    /// <param name="onTextReceived">The callback to handle the streaming chunks</param>
    /// <param name="model">Ollama Model Syntax (<b>eg.</b> llama3.1)</param>
    /// <param name="image">A multimodal model is required to handle images (<b>eg.</b> llava)</param>
    /// <param name="keep_alive">The behavior to keep the model loaded in memory</param>
    public static async Task ChatStream(Action<string> onTextReceived, string model, string prompt, Texture2D image = null, KeepAlive keep_alive = KeepAlive.five_minute)
    {
        ChatHistory.Enqueue(new Message("user", prompt, Texture2Base64(image)));
        Message[] messages;

        if (systemPrompt == null)
            messages = ChatHistory.ToArray();
        else
        {
            messages = new Message[ChatHistory.Count + 1];
            messages[0] = systemPrompt;
            ChatHistory.CopyTo(messages, 1);
        }

        var request = new Request.Chat(model, messages, true, keep_alive);
        string payload = JsonConvert.SerializeObject(request);
        StringBuilder reply = new StringBuilder();

        await PostRequestStream(payload, Endpoints.CHAT, (Response.Chat response) =>
        {
            if (!response.done)
            {
                onTextReceived?.Invoke(response.message.content);
                reply.Append(response.message.content);
            }
        });

        ChatHistory.Enqueue(new Message("assistant", reply.ToString()));
        while (ChatHistory.Count > HistoryLimit)
            ChatHistory.Dequeue();

        OnStreamFinished?.Invoke();
    }

    /// <summary>Set a <b>System</b> prompt that is always active</summary>
    public static void SetSystemPrompt(string system) { systemPrompt = new Message("system", system); }
    /// <summary>Remove the <b>System</b> prompt</summary>
    public static void RemoveSystemPrompt() { systemPrompt = null; }
}

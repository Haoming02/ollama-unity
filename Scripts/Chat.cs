using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public static partial class Ollama
{
    private static List<Message> ChatHistory;

    /// <summary> Start a brand new chat </summary>
    public static void InitChat() => ChatHistory = new List<Message>();

    /// <summary> Save the current Chat History to the specified path </summary>
    public static void SaveChatHistory(string fileName = null)
    {
        if (fileName == null)
            fileName = Path.Combine(UnityEngine.Application.persistentDataPath, "chat.dat");

        using var stream = File.Open(fileName, FileMode.Create);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, false);

        var data = JsonConvert.SerializeObject(ChatHistory);
        writer.Write(IO.Encrypt(data));
        UnityEngine.Debug.Log($"Chat History saved to \"{fileName}\"");
    }

    /// <summary> Load a Chat History from the specified path </summary>
    public static void LoadChatHistory(string fileName = null)
    {
        if (fileName == null)
            fileName = Path.Combine(UnityEngine.Application.persistentDataPath, "chat.dat");

        if (File.Exists(fileName))
        {
            using var stream = File.Open(fileName, FileMode.Open);
            using var reader = new BinaryReader(stream, Encoding.UTF8, false);

            ChatHistory = JsonConvert.DeserializeObject<List<Message>>(IO.Decrypt(reader.ReadString()));
            UnityEngine.Debug.Log($"Chat History loaded from \"{fileName}\"");
        }
        else
        {
            UnityEngine.Debug.LogWarning($"Chat History \"{fileName}\" does not exist!");
            InitChat();
        }
    }

    /// <summary> Generate a response for a given prompt using a provided model with history </summary>
    public static async Task<string> Chat(string prompt, string system = null, string model = "llama3")
    {
        if (system != null)
            ChatHistory.Add(new Message("system", system));

        ChatHistory.Add(new Message("user", prompt));

        var request = new Request.Chat(model, ChatHistory.ToArray(), false);
        string payload = JsonConvert.SerializeObject(request);
        var response = await PostRequest<Response.Chat>(payload, Endpoints.CHAT);

        ChatHistory.Add(response.message);

        return response.message.content;
    }

    /// <summary> Stream a response for a given prompt using a provided model with history </summary>
    public static async Task ChatStream(string prompt, Action<string> onTextReceived, string system = null, string model = "llama3")
    {
        if (system != null)
            ChatHistory.Add(new Message("system", system));

        ChatHistory.Add(new Message("user", prompt));

        var request = new Request.Chat(model, ChatHistory.ToArray(), true);
        string payload = JsonConvert.SerializeObject(request);

        StringBuilder reply = new StringBuilder();

        await PostRequestStream(payload, Endpoints.CHAT, (Response.Chat response) =>
        {
            onTextReceived?.Invoke(response.message.content);
            reply.Append(response.message.content);
        });

        ChatHistory.Add(new Message("assistant", reply.ToString()));
    }
}

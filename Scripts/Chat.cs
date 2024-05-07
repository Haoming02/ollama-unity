using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Text;

public static partial class Ollama
{
    private static List<Message> ChatHistory;

    /// <summary> Start a brand new chat </summary>
    public static void InitChat() => ChatHistory = new List<Message>();

    public static void SaveChatHistory() => throw new NotImplementedException();
    public static void LoadChatHistory() => throw new NotImplementedException();

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

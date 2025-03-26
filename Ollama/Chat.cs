using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ollama
{
    public static partial class Ollama
    {
        private static List<Message> ChatHistory;
        private static int HistoryLimit;

        /// <summary>Start a brand new chat</summary>
        /// <param name="historyLimit">Number of messages to keep in memory <i>(includes both prompt and response, but <b>not</b> system)</i></param>
        /// <param name="system">Add a <b>System</b> prompt that is always active</param>
        public static void InitChat(int historyLimit = 8, string system = null)
        {
            if (ChatHistory == null)
                ChatHistory = new List<Message>();
            else
                ChatHistory.Clear();

            HistoryLimit = historyLimit;

            if (!string.IsNullOrEmpty(system))
                SetSystemPrompt(system);
        }

        /// <summary>Save the current Chat History</summary>
        /// <param name="fileName">If not provided, defaults to <see href="https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html">Application.persistentDataPath</see></param>
        public static void SaveChatHistory(string fileName = null)
        {
            if (string.IsNullOrEmpty(fileName))
                fileName = Path.Combine(Application.persistentDataPath, "chat.dat");

            var data = JsonConvert.SerializeObject(ChatHistory);

            using (var stream = File.Open(fileName, FileMode.Create))
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, false))
                writer.Write(IO.Encrypt(data));

            Debug.Log($"Saved Chat History to \"{fileName}\"");
        }

        /// <summary>Load a Chat History</summary>
        /// <param name="fileName">If not provided, defaults to <see href="https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html">Application.persistentDataPath</see></param>
        /// <param name="historyLimit">Number of messages to keep in memory <i>(includes both prompt and response, but <b>not</b> system)</i></param>
        public static void LoadChatHistory(string fileName = null, int historyLimit = 8)
        {
            if (string.IsNullOrEmpty(fileName))
                fileName = Path.Combine(Application.persistentDataPath, "chat.dat");

            if (!File.Exists(fileName))
            {
                InitChat(historyLimit);
                Debug.LogWarning($"Chat History \"{fileName}\" does not exist...");
                return;
            }

            string data;

            using (var stream = File.Open(fileName, FileMode.Open))
            using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
                data = reader.ReadString();

            ChatHistory = JsonConvert.DeserializeObject<List<Message>>(IO.Decrypt(data));
            HistoryLimit = historyLimit;

            bool system = HasSystemPrompt();

            int _limit = HistoryLimit + (system ? 1 : 0);
            while (ChatHistory.Count > _limit)
            {
                if (system)
                    ChatHistory.RemoveAt(1);
                else
                    ChatHistory.RemoveAt(0);
            }

            Debug.Log($"Loaded Chat History from \"{fileName}\"");
        }

        /// <summary>Generate a response from prompt, with chat context/history</summary>
        /// <param name="model">Ollama Model Syntax (<b>eg.</b> gemma3:4b)</param>
        /// <param name="keep_alive">The duration <i>(in seconds)</i> to keep the model in memory</param>
        /// <returns>response string from the LLM</returns>
        public static async Task<string> Chat(string model, string prompt, int keep_alive = 300, Texture2D image = null)
        {
            ChatHistory.Add(new Message("user", prompt, Texture2Base64(image)));

            var request = new Request.Chat(model, ChatHistory, false, keep_alive);
            string payload = JsonConvert.SerializeObject(request);
            var response = await PostRequest<Response.Chat>(payload, Endpoints.CHAT);

            ChatHistory.Add(response.message);

            bool system = HasSystemPrompt();

            int _limit = HistoryLimit + (system ? 1 : 0);
            while (ChatHistory.Count > _limit)
            {
                if (system)
                    ChatHistory.RemoveAt(1);
                else
                    ChatHistory.RemoveAt(0);
            }

            return response.message.content;
        }

        /// <summary>Stream a response from prompt, with chat context/history</summary>
        /// <param name="onTextReceived">The callback to handle the streaming chunks</param>
        /// <param name="model">Ollama Model Syntax (<b>eg.</b> gemma3:4b)</param>
        /// <param name="keep_alive">The duration <i>(in seconds)</i> to keep the model in memory</param>
        public static async Task ChatStream(Action<string> onTextReceived, string model, string prompt, int keep_alive = 300, Texture2D image = null)
        {
            ChatHistory.Add(new Message("user", prompt, Texture2Base64(image)));

            var request = new Request.Chat(model, ChatHistory, true, keep_alive);
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

            ChatHistory.Add(new Message("assistant", reply.ToString()));

            bool system = HasSystemPrompt();

            int _limit = HistoryLimit + (system ? 1 : 0);
            while (ChatHistory.Count > _limit)
            {
                if (system)
                    ChatHistory.RemoveAt(1);
                else
                    ChatHistory.RemoveAt(0);
            }

            OnStreamFinished?.Invoke();
        }


        public static bool HasSystemPrompt() => (ChatHistory != null) && (ChatHistory.Count > 0) && (ChatHistory[0].role == "system");

        public static void SetSystemPrompt(string system)
        {
            if (HasSystemPrompt())
                ChatHistory[0] = new Message("system", system);
            else
                ChatHistory.Insert(0, new Message("system", system));
        }

        public static string GetSystemPrompt()
        {
            return HasSystemPrompt() ? ChatHistory[0].content : null;
        }

        public static void RemoveSystemPrompt()
        {
            if (HasSystemPrompt()) ChatHistory.RemoveAt(0);
        }
    }
}

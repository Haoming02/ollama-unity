using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace ollama
{
    public static partial class Ollama
    {
        /// <summary>Generate a simple response from prompt</summary>
        /// <param name="model">Ollama Model Syntax (<b>eg.</b> gemma3:4b)</param>
        /// <param name="keep_alive">The duration <i>(in seconds)</i> to keep the model in memory</param>
        /// <returns>response string from the LLM</returns>
        public static async Task<string> Generate(string model, string prompt, int keep_alive = 300, Texture2D[] images = null)
        {
            var imagesBase64 = EncodeTextures(images);
            var request = new Request.Generate(model, prompt, false, keep_alive, null, imagesBase64);
            string payload = JsonConvert.SerializeObject(request);
            var response = await PostRequest<Response.Generate>(payload, Endpoints.GENERATE);
            return response.response;
        }

        /// <summary>Generate a simple response in JSON format from prompt</summary>
        /// <typeparam name="T">The class/struct of the JSON object</typeparam>
        /// <param name="model">Ollama Model Syntax (<b>eg.</b> gemma3:4b)</param>
        /// <param name="prompt"><b>Important:</b> You need to manaully tell the LLM to reply in the JSON format</param>
        /// <param name="keep_alive">The duration <i>(in seconds)</i> to keep the model in memory</param>
        /// <returns>response object from the LLM</returns>
        public static async Task<T> GenerateJson<T>(string model, string prompt, int keep_alive = 300, Texture2D[] images = null)
        {
            var imagesBase64 = EncodeTextures(images);
            var request = new Request.Generate(model, prompt, false, keep_alive, "json", imagesBase64);
            string payload = JsonConvert.SerializeObject(request);
            var response = await PostRequest<Response.Generate>(payload, Endpoints.GENERATE);
            return JsonConvert.DeserializeObject<T>(response.response);
        }

        /// <summary>Stream a simple response from prompt</summary>
        /// <param name="onTextReceived">The callback to handle the streaming chunks</param>
        /// <param name="model">Ollama Model Syntax (<b>eg.</b> gemma3:4b)</param>
        /// <param name="keep_alive">The duration <i>(in seconds)</i> to keep the model in memory</param>
        public static async Task GenerateStream(Action<string> onTextReceived, string model, string prompt, int keep_alive = 300, Texture2D[] images = null)
        {
            var imagesBase64 = EncodeTextures(images);
            var request = new Request.Generate(model, prompt, true, keep_alive, null, imagesBase64);
            string payload = JsonConvert.SerializeObject(request);

            await PostRequestStream(payload, Endpoints.GENERATE, (Response.Generate response) =>
            {
                if (!response.done)
                    onTextReceived?.Invoke(response.response);
            });

            OnStreamFinished?.Invoke();
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using UnityEngine;

public static partial class Ollama
{
    /// <summary>
    /// Generate a simple response from prompt <i>(no context/history)</i>
    /// </summary>
    /// <param name="model">Ollama Model Syntax (<b>eg.</b> llama3.1)</param>
    /// <param name="images">A multimodal model is required to handle images (<b>eg.</b> llava)</param>
    /// <param name="keep_alive">The behavior to keep the model loaded in memory</param>
    /// <returns>response string from the LLM</returns>
    public static async Task<string> Generate(string model, string prompt, Texture2D[] images = null, KeepAlive keep_alive = KeepAlive.five_minute)
    {
        var imagesBase64 = EncodeTextures(images);
        var request = new Request.Generate(model, prompt, imagesBase64, null, false, keep_alive);
        string payload = JsonConvert.SerializeObject(request);
        var response = await PostRequest<Response.Generate>(payload, Endpoints.GENERATE);
        return response.response;
    }

    /// <summary>
    /// Generate a simple response in JSON format from prompt
    /// </summary>
    /// <typeparam name="T">The structure of the JSON object</typeparam>
    /// <param name="model">Ollama Model Syntax (<b>eg.</b> llama3.1)</param>
    /// <param name="prompt">You need to manaully tell the LLM to return in a JSON format</param>
    /// <param name="images">A multimodal model is required to handle images (<b>eg.</b> llava)</param>
    /// <param name="keep_alive">The behavior to keep the model loaded in memory</param>
    /// <returns>response string from the LLM</returns>
    public static async Task<T> GenerateJson<T>(string model, string prompt, Texture2D[] images = null, KeepAlive keep_alive = KeepAlive.five_minute)
    {
        var imagesBase64 = EncodeTextures(images);
        var request = new Request.Generate(model, prompt, imagesBase64, "json", false, keep_alive);
        string payload = JsonConvert.SerializeObject(request);
        var response = await PostRequest<Response.Generate>(payload, Endpoints.GENERATE);
        return JsonConvert.DeserializeObject<T>(response.response);
    }

    /// <summary>
    /// Stream a simple response from prompt <i>(no context/history)</i>
    /// </summary>
    /// <param name="onTextReceived">The callback to handle the streaming chunks</param>
    /// <param name="model">Ollama Model Syntax (<b>eg.</b> llama3.1)</param>
    /// <param name="images">A multimodal model is required to handle images (<b>eg.</b> llava)</param>
    /// <param name="keep_alive">The behavior to keep the model loaded in memory</param>
    public static async Task GenerateStream(Action<string> onTextReceived, string model, string prompt, Texture2D[] images = null, KeepAlive keep_alive = KeepAlive.five_minute)
    {
        var imagesBase64 = EncodeTextures(images);
        var request = new Request.Generate(model, prompt, imagesBase64, null, true, keep_alive);
        string payload = JsonConvert.SerializeObject(request);
        await PostRequestStream(payload, Endpoints.GENERATE, (Response.Generate response) =>
        {
            if (!response.done)
                onTextReceived?.Invoke(response.response);
        });

        OnStreamFinished?.Invoke();
    }
}

using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

public static partial class Ollama
{
    /// <summary> Generate a response for a given prompt with a provided model </summary>
    public static async Task<string> Generate(string prompt, string model = "llama3")
    {
        var request = new Request.Generate(model, prompt, false);
        string payload = JsonConvert.SerializeObject(request);
        var response = await PostRequest<Response.Generate>(payload, Endpoints.GENERATE);
        return response.response;
    }

    /// <summary> Generate a response for a given prompt with a provided model </summary>
    public static async Task<T> GenerateJson<T>(string prompt, string model = "llama3")
    {
        var request = new Request.Generate(model, prompt, false, "json");
        string payload = JsonConvert.SerializeObject(request);
        var response = await PostRequest<Response.Generate>(payload, Endpoints.GENERATE);
        return JsonConvert.DeserializeObject<T>(response.response);
    }

    /// <summary> Stream a response for a given prompt with a provided model </summary>
    public static async Task GenerateStream(string prompt, Action<string> onTextReceived, string model = "llama3")
    {
        var request = new Request.Generate(model, prompt, true);
        string payload = JsonConvert.SerializeObject(request);
        await PostRequestStream(payload, Endpoints.GENERATE, (Response.Generate response) => { onTextReceived?.Invoke(response.response); });
        OnStreamFinished?.Invoke();
    }


    /// <summary> Generate a response for a given prompt and image from a multimodal model </summary>
    public static async Task<string> GenerateWithImage(string prompt, string image, string model = "llava")
    {
        var request = new Request.Generate(model, prompt, false, null, new string[] { image });
        string payload = JsonConvert.SerializeObject(request);
        var response = await PostRequest<Response.Generate>(payload, Endpoints.GENERATE);
        return response.response;
    }

    /// <summary> Stream a response for a given prompt and image from a multimodal model </summary>
    public static async Task GenerateWithImageStream(string prompt, string image, Action<string> onTextReceived, string model = "llava")
    {
        var request = new Request.Generate(model, prompt, true, null, new string[] { image });
        string payload = JsonConvert.SerializeObject(request);
        await PostRequestStream(payload, Endpoints.GENERATE, (Response.Generate response) => { onTextReceived?.Invoke(response.response); });
        OnStreamFinished?.Invoke();
    }
}

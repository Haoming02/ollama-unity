using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static partial class Ollama
{
    /// <summary>Subscribe to this Event to know when a Streaming response is finished</summary>
    public static Action OnStreamFinished;

    private const string SERVER = "http://localhost:11434/";

    private static class Endpoints
    {
        public const string GENERATE = "api/generate";
        public const string CHAT = "api/chat";
        public const string LIST = "api/tags";
        public const string EMBEDDINGS = "api/embed";
    }

    private static async Task<T> PostRequest<T>(string payload, string endpoint)
    {
        HttpWebRequest httpWebRequest;

        try
        {
            httpWebRequest = (HttpWebRequest)WebRequest.Create($"{SERVER}{endpoint}");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using var streamWriter = new StreamWriter(await httpWebRequest.GetRequestStreamAsync());
            streamWriter.Write(payload);
        }
        catch (Exception e)
        {
            Debug.LogError($"{e.Message}\n\t{e.StackTrace}");
            return default;
        }

        var httpResponse = await httpWebRequest.GetResponseAsync();
        using var streamReader = new StreamReader(httpResponse.GetResponseStream());

        string result = await streamReader.ReadToEndAsync();
        return JsonConvert.DeserializeObject<T>(result);
    }

    private static async Task PostRequestStream<T>(string payload, string endpoint, Action<T> onChunkReceived) where T : Response.BaseResponse
    {
        HttpWebRequest httpWebRequest;

        try
        {
            httpWebRequest = (HttpWebRequest)WebRequest.Create($"{SERVER}{endpoint}");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using var streamWriter = new StreamWriter(await httpWebRequest.GetRequestStreamAsync());
            streamWriter.Write(payload);
        }
        catch (Exception e)
        {
            Debug.LogError($"{e.Message}\n\t{e.StackTrace}");
            return;
        }

        using var httpResponse = await httpWebRequest.GetResponseAsync();
        using var responseStream = httpResponse.GetResponseStream();

        StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
        bool isEnd = false;

        while (!isEnd)
        {
            string result = await reader.ReadLineAsync();
            var response = JsonConvert.DeserializeObject<T>(result);
            onChunkReceived?.Invoke(response);
            isEnd = response.done;
        }
    }

    private static async Task<T> GetRequest<T>(string endpoint)
    {
        HttpWebRequest httpWebRequest;

        try
        {
            httpWebRequest = (HttpWebRequest)WebRequest.Create($"{SERVER}{endpoint}");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";
        }
        catch (Exception e)
        {
            Debug.LogError($"{e.Message}\n\t{e.StackTrace}");
            return default;
        }

        var httpResponse = await httpWebRequest.GetResponseAsync();
        using var streamReader = new StreamReader(httpResponse.GetResponseStream());

        string result = await streamReader.ReadToEndAsync();
        return JsonConvert.DeserializeObject<T>(result);
    }
}

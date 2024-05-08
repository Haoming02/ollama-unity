using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static partial class Ollama
{
    private const string SERVER = "http://localhost:11434/";

    private static class Endpoints
    {
        public const string GENERATE = "api/generate";
        public const string CHAT = "api/chat";
        public const string LIST = "api/tags";
    }

    public static Action OnStreamFinished;

    private static async Task<T> PostRequest<T>(string payload, string endpoint) where T : Response.BaseResponse
    {
        HttpWebRequest httpWebRequest;

        try
        {
            httpWebRequest = (HttpWebRequest)WebRequest.Create($"{SERVER}{endpoint}");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream());
            streamWriter.Write(payload);
        }
        catch (Exception e)
        {
            Debug.LogError($"{e.Message}\n\t{e.StackTrace}");
            return default;
        }

        var httpResponse = await httpWebRequest.GetResponseAsync();

        using var streamReader = new StreamReader(httpResponse.GetResponseStream());

        string result = streamReader.ReadToEnd();
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

            using var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream());
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
            isEnd = response.done;

            if (!isEnd)
                onChunkReceived?.Invoke(response);
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

        string result = streamReader.ReadToEnd();
        return JsonConvert.DeserializeObject<T>(result);
    }
}

using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public static partial class Ollama
{
    private const string SERVER = "http://localhost:11434/";

    private static class Endpoints
    {
        public const string GENERATE = "api/generate";
        public const string LIST = "api/tags";
    }

    private static async Task<T> PostRequest<T>(string payload, string endpoint)
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
        var response = JsonConvert.DeserializeObject<T>(result);
        return response;
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
        var response = JsonConvert.DeserializeObject<T>(result);
        return response;
    }
}

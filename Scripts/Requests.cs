using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public static partial class Ollama
{
    private const string SERVER = "http://localhost:11434/";
    private const string GENERATE = "api/generate";

    private static async Task<Response> SendRequest(string payload)
    {
        HttpWebRequest httpWebRequest;

        try
        {
            httpWebRequest = (HttpWebRequest)WebRequest.Create($"{SERVER}{GENERATE}");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream());
            streamWriter.Write(payload);
        }
        catch (Exception e)
        {
            Debug.LogError($"{e.Message}\n\t{e.StackTrace}");
            return null;
        }

        var httpResponse = await httpWebRequest.GetResponseAsync();

        using var streamReader = new StreamReader(httpResponse.GetResponseStream());

        string result = streamReader.ReadToEnd();
        Response response = JsonConvert.DeserializeObject<Response>(result);
        return response;
    }
}

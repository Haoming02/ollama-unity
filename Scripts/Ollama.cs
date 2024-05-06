using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public static class Ollama
{
    private const string SERVER = "http://localhost:11434/";
    private const string GENERATE = "api/generate";

    public static async Task<string> Chat(string input, string model = "llama3")
    {
        HttpWebRequest httpWebRequest;

        try
        {
            httpWebRequest = (HttpWebRequest)WebRequest.Create($"{SERVER}{GENERATE}");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                Request request = new Request(model, input, false);
                string payload = JsonConvert.SerializeObject(request);
                streamWriter.Write(payload);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"{e.Message}\n\t{e.StackTrace}");
            return null;
        }

        var httpResponse = await httpWebRequest.GetResponseAsync();

        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            string result = streamReader.ReadToEnd();
            Response response = JsonConvert.DeserializeObject<Response>(result);
            return response.response;
        }
    }

    private class Request
    {
        public string model;
        public string prompt;
        public bool stream;

        public Request(string model, string prompt, bool stream)
        {
            this.model = model;
            this.prompt = prompt;
            this.stream = stream;
        }
    }

    public class Response
    {
        public string model;
        public DateTime created_at;
        public string response;
        public bool done;
        public int[] context;
        public long total_duration;
        public long load_duration;
        public int prompt_eval_count;
        public long prompt_eval_duration;
        public int eval_count;
        public long eval_duration;
    }
}

using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ollama
{
    /// <summary> https://github.com/ollama/ollama/blob/main/docs/api.md </summary>
    public static partial class Ollama
    {
        private const string SERVER = "http://localhost:11434/";

        private static class Endpoints
        {
            public const string GENERATE = "api/generate";
            public const string CHAT = "api/chat";
            public const string LIST = "api/tags";
            public const string EMBEDDINGS = "api/embed";
        }

        private static async Task<T> PostRequest<T>(string payload, string endpoint) where T : Response.Base
        {
            HttpWebRequest httpWebRequest;

            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create($"{SERVER}{endpoint}");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(await httpWebRequest.GetRequestStreamAsync().ConfigureAwait(false)))
                    await streamWriter.WriteAsync(payload).ConfigureAwait(false);

                string result;

                using (var httpResponse = await httpWebRequest.GetResponseAsync().ConfigureAwait(false))
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    result = await streamReader.ReadToEndAsync().ConfigureAwait(false);

                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during \"{endpoint}\" PostRequest:\n{e.Message}\n{e.StackTrace}");
                return default;
            }
        }

        private static async Task PostRequestStream<T>(string payload, string endpoint, Action<T> onChunkReceived) where T : Response.BaseResponse
        {
            HttpWebRequest httpWebRequest;

            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create($"{SERVER}{endpoint}");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(await httpWebRequest.GetRequestStreamAsync().ConfigureAwait(false)))
                    await streamWriter.WriteAsync(payload).ConfigureAwait(false);

                bool isEnd = false;
                int it = 0;

                using (var httpResponse = await httpWebRequest.GetResponseAsync().ConfigureAwait(false))
                using (var responseStream = httpResponse.GetResponseStream())
                using (var reader = new StreamReader(responseStream, Encoding.UTF8))
                    while (!isEnd)
                    {
                        it++;
                        string result = await reader.ReadLineAsync().ConfigureAwait(false);
                        var response = JsonConvert.DeserializeObject<T>(result);
                        if (it > MaxIterations)
                        {
                            Debug.LogError($"Stream has reached {MaxIterations} iterations... Probably server error?");
                            response.done = true;
                        }

                        onChunkReceived?.Invoke(response);
                        isEnd = response.done;
                    }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during \"{endpoint}\" PostRequestStream:\n{e.Message}\n{e.StackTrace}");
                return;
            }
        }

        private static async Task<T> GetRequest<T>(string endpoint) where T : Response.Base
        {
            HttpWebRequest httpWebRequest;

            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create($"{SERVER}{endpoint}");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";

                string result;

                using (var httpResponse = await httpWebRequest.GetResponseAsync().ConfigureAwait(false))
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    result = await streamReader.ReadToEndAsync().ConfigureAwait(false);

                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during \"{endpoint}\" GetRequest:\n{e.Message}\n{e.StackTrace}");
                return default;
            }
        }
    }
}

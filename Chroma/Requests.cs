using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public static partial class ChromaDB
{
    private const string SERVER = "http://localhost:8000";
    private static string AUTH_TOKEN = null;

    private static class Endpoints
    {
        public const string HEARTBEAT = "/api/v1/heartbeat";
        public const string COLLECTIONS = "/api/v1/collections";
        public const string COUNT_COLLECTIONS = "/api/v1/count_collections";

        public const string ADD = "/api/v1/collections/{id}/add";
        public const string GET = "/api/v1/collections/{id}/get";
        public const string QUERY = "/api/v1/collections/{id}/query";
        public const string RESET = "/api/v1/reset";
    }

    private static async Task<T> PostRequest<T>(string payload, string endpoint)
    {
        HttpWebRequest httpWebRequest;

        try
        {
            httpWebRequest = (HttpWebRequest)WebRequest.Create($"{SERVER}{endpoint}");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            if (AUTH_TOKEN != null)
                httpWebRequest.Headers.Add("Authorization", $"Bearer {AUTH_TOKEN}");

            if (payload != null)
            {
                using var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream());
                streamWriter.Write(payload);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"{e.Message}\n\t{e.StackTrace}");
            return default;
        }

        return await SendRequest<T>(httpWebRequest);
    }

    private static async Task<T> GetRequest<T>(string endpoint)
    {
        HttpWebRequest httpWebRequest;

        try
        {
            httpWebRequest = (HttpWebRequest)WebRequest.Create($"{SERVER}{endpoint}");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";

            if (AUTH_TOKEN != null)
                httpWebRequest.Headers.Add("Authorization", $"Bearer {AUTH_TOKEN}");
        }
        catch (Exception e)
        {
            Debug.LogError($"{e.Message}\n\t{e.StackTrace}");
            return default;
        }

        return await SendRequest<T>(httpWebRequest);
    }

    internal static async Task<T> SendRequest<T>(HttpWebRequest httpWebRequest)
    {
        try
        {
            var httpResponse = await httpWebRequest.GetResponseAsync();
            using var streamReader = new StreamReader(httpResponse.GetResponseStream());
            string result = streamReader.ReadToEnd();
            return JsonConvert.DeserializeObject<T>(result);
        }
        catch (WebException e)
        {
            if (e.Status == WebExceptionStatus.ConnectFailure)
                throw new HttpListenerException();

            var status = (e.Response as HttpWebResponse).StatusCode;
            var resp = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();

            switch (status)
            {
                case HttpStatusCode.Forbidden:
                    Debug.LogError("Incorrect Auth Token");
                    break;
                case HttpStatusCode.InternalServerError:
                case HttpStatusCode.BadRequest:
                    Debug.LogError($"Incorrect Syntax\n{resp}");
                    break;
                default:
                    Debug.LogError($"{resp}\n{e.Message}\n{e.StackTrace}");
                    break;
            }

            return default;
        }
    }
}

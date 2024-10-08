using Newtonsoft.Json;
using System.Threading.Tasks;
using Unity.Mathematics;
using static Ollama;

public static partial class Ollama
{
    /// <summary>
    /// Generate embeddings from a string using given model <br/>
    /// <b>Note:</b> Not all models can generate embeddings
    /// </summary>
    /// <param name="model">Ollama Model Syntax (<b>eg.</b> llama3.1)</param>
    /// <param name="input">prompt</param>
    /// <returns>embeddings</returns>
    public static async Task<float[]> Embeddings(string model, string input, KeepAlive keep_alive = KeepAlive.five_minute)
    {
        var request = new Request.Embeddings(model, input, keep_alive);
        string payload = JsonConvert.SerializeObject(request);
        var response = await PostRequest<Response.Embeddings>(payload, Endpoints.EMBEDDINGS);
        return response.embeddings[0];
    }

    /// <summary>
    /// Generate embeddings from a string using given model <br/>
    /// <b>Note:</b> Not all models can generate embeddings
    /// </summary>
    /// <param name="model">Ollama Model Syntax (<b>eg.</b> llama3.1)</param>
    /// <param name="input">array of prompts</param>
    /// <returns>array of embeddings</returns>
    public static async Task<float[][]> Embeddings(string model, string[] input, KeepAlive keep_alive = KeepAlive.five_minute)
    {
        var request = new Request.Embeddings(model, input, keep_alive);
        string payload = JsonConvert.SerializeObject(request);
        var response = await PostRequest<Response.Embeddings>(payload, Endpoints.EMBEDDINGS);
        return response.embeddings;
    }

    public static float CosineSimilarity(float[] V1, float[] V2)
    {
        int N = V1.Length;

        float dot = 0.0f;
        float mag1 = 0.0f;
        float mag2 = 0.0f;

        for (int n = 0; n < N; n++)
        {
            dot += V1[n] * V2[n];
            mag1 += V1[n] * V1[n];
            mag2 += V2[n] * V2[n];
        }

        return dot / math.sqrt(mag1 * mag2);
    }
}

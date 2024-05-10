using Newtonsoft.Json;
using System.Threading.Tasks;
using Unity.Mathematics;

public static partial class Ollama
{
    /// <summary> Generate an embeddings for a given prompt with a provided model </summary>
    public static async Task<float[]> Embeddings(string prompt, string model = "nomic-embed-text")
    {
        var request = new Request.Embeddings(model, prompt);
        string payload = JsonConvert.SerializeObject(request);
        var response = await PostRequest<Response.Embeddings>(payload, Endpoints.EMBEDDINGS);
        return response.embedding;
    }

    public static float CosineSimilarity(float[] V1, float[] V2)
    {
        int N = math.min(V1.Length, V2.Length);

        float dot = 0.0f;
        float mag1 = 0.0f;
        float mag2 = 0.0f;

        for (int n = 0; n < N; n++)
        {
            dot += V1[n] * V2[n];
            mag1 += math.pow(V1[n], 2);
            mag2 += math.pow(V2[n], 2);
        }

        return dot / (math.sqrt(mag1) * math.sqrt(mag2));
    }
}

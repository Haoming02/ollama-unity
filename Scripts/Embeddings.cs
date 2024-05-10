using Newtonsoft.Json;
using System.Threading.Tasks;
using Unity.Mathematics;

public static partial class Ollama
{
    /// <summary> Generate an embeddings for a given prompt with a provided model </summary>
    public static async Task<double[]> Embeddings(string prompt, string model = "llama3")
    {
        var request = new Request.Embeddings(model, prompt);
        string payload = JsonConvert.SerializeObject(request);
        var response = await PostRequest<Response.Embeddings>(payload, Endpoints.EMBEDDINGS);
        return response.embedding;
    }

    public static double CosineSimilarity(double[] V1, double[] V2)
    {
        int N = math.min(V1.Length, V2.Length);

        double dot = 0.0;
        double mag1 = 0.0;
        double mag2 = 0.0;

        for (int n = 0; n < N; n++)
        {
            dot += V1[n] * V2[n];
            mag1 += math.pow(V1[n], 2);
            mag2 += math.pow(V2[n], 2);
        }

        return dot / (math.sqrt(mag1) * math.sqrt(mag2));
    }
}

using Newtonsoft.Json;
using System.Threading.Tasks;

public static partial class Ollama
{
    /// <summary> Generate an embeddings for a given prompt with a provided model </summary>
    public static async Task<float[]> Embeddings(string prompt, string model = "mxbai-embed-large")
    {
        var request = new Request.Embeddings(model, prompt);
        string payload = JsonConvert.SerializeObject(request);
        var response = await PostRequest<Response.Embeddings>(payload, Endpoints.EMBEDDINGS);
        return response.embedding;
    }
}

using Newtonsoft.Json;
using System.Threading.Tasks;
using Unity.Mathematics;

namespace ollama
{
    public static partial class Ollama
    {
        /// <summary>Generate embeddings from a string using given model</summary>
        /// <param name="model">Ollama Model Syntax (<b>eg.</b> gemma3:4b)</param>
        /// <param name="keep_alive">The duration <i>(in seconds)</i> to keep the model in memory</param>
        /// <returns>embeddings</returns>
        public static async Task<float[]> Embeddings(string model, string prompt, int keep_alive = 300)
        {
            var request = new Request.Embeddings(model, prompt, keep_alive);
            string payload = JsonConvert.SerializeObject(request);
            var response = await PostRequest<Response.Embeddings>(payload, Endpoints.EMBEDDINGS);
            return response.embeddings[0];
        }

        /// <summary>Replace this with better algorithm for improved accuracy...</summary>
        private static float CosineSimilarity(float[] V1, float[] V2)
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
}

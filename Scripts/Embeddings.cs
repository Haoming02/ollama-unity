using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using Unity.Mathematics;

public static partial class Ollama
{
    public enum NormalizationMode
    {
        none,
        zero_one,
        one_one,
        mean_std
    }

    /// <summary> Generate an embeddings for a given prompt with a provided model </summary>
    public static async Task<double[]> Embeddings(string prompt, string model = "nomic-embed-text", NormalizationMode normalization = NormalizationMode.mean_std)
    {
        var request = new Request.Embeddings(model, prompt);
        string payload = JsonConvert.SerializeObject(request);
        var response = await PostRequest<Response.Embeddings>(payload, Endpoints.EMBEDDINGS);

        return response.embedding.Normalized(normalization);
    }

    private static double[] Normalized(this double[] data, NormalizationMode mode)
    {
        int len = data.Length;
        double min, max, range;

        switch (mode)
        {
            case NormalizationMode.zero_one:
                min = data.Min();
                max = data.Max();
                range = max - min;

                for (int i = 0; i < len; i++)
                    data[i] = (data[i] - min) / range;

                return data;

            case NormalizationMode.one_one:
                min = data.Min();
                max = data.Max();
                range = max - min;

                for (int i = 0; i < len; i++)
                    data[i] = ((data[i] - min) / range) * 2.0 - 1.0;

                return data;

            case NormalizationMode.mean_std:
                double mean = data.Average();
                double sumSquaredDifferences = 0.0;

                foreach (double num in data)
                    sumSquaredDifferences += math.pow(num - mean, 2);

                double std = math.sqrt(sumSquaredDifferences / len);

                for (int i = 0; i < len; i++)
                    data[i] = (data[i] - mean) / std;

                return data;

            default:
                return data;
        }
    }
}

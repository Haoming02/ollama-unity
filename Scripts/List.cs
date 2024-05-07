using System;
using System.Threading.Tasks;

public static partial class Ollama
{
    public static async Task<string[]> List()
    {
        var response = await GetRequest<ListResponse>(Endpoints.LIST);

        string[] models = new string[response.models.Length];
        for (int i = 0; i < response.models.Length; i++)
            models[i] = response.models[i].name;

        return models;
    }

    private class ListResponse
    {
        public Model[] models;

        public class Model
        {
            public string name;
            public DateTime modified_at;
            public long size;
            public string digest;
            public ModelDetail details;

            public class ModelDetail
            {
                public string format;
                public string family;
                public string[] families;
                public string parameter_size;
                public string quantization_level;
            }
        }
    }
}

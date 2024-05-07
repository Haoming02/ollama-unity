using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static partial class Ollama
{
    /// <summary> List models that are available locally </summary>
    public static async Task<string[]> List()
    {
        var response = await GetRequest<Response.List>(Endpoints.LIST);

        int l = response.models.Length;
        string[] models = new string[l];
        for (int i = 0; i < l; i++)
            models[i] = response.models[i].name;

        return models;
    }

    /// <summary> List models that are available locally, categorized by types </summary>
    /// <returns>(regular models, multimodal models)</returns>
    public static async Task<(string[], string[])> Lists()
    {
        var response = await GetRequest<Response.List>(Endpoints.LIST);
        int l = response.models.Length;

        List<string> models = new List<string>();
        List<string> mm_models = new List<string>();

        for (int i = 0; i < l; i++)
        {
            if (Array.Exists(response.models[i].details.families, family => family != "llama"))
                mm_models.Add(response.models[i].name);
            else
                models.Add(response.models[i].name);
        }

        return (models.ToArray(), mm_models.ToArray());
    }
}

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

    /// <summary> List models that are available locally, categorized by family </summary>
    /// <returns> (regular models, multimodal models) </returns>
    public static async Task<(string[], string[])> ListCategorized()
    {
        var response = await GetRequest<Response.List>(Endpoints.LIST);

        List<string> text_model = new List<string>();
        List<string> multimodal = new List<string>();

        int l = response.models.Length;
        for (int i = 0; i < l; i++)
        {
            if (!Array.Exists(response.models[i].details.families, family => family != "llama"))
                text_model.Add(response.models[i].name);
            else
                multimodal.Add(response.models[i].name);
        }

        return (text_model.ToArray(), multimodal.ToArray());
    }
}

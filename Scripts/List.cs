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
}

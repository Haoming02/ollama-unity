using System.Threading.Tasks;

public static partial class Ollama
{
    /// <summary>
    /// Return a list of all locally available models <br/>
    /// Use <b>model.families</b> to determine whether a model is multimodal or not
    /// </summary>
    /// <returns>an array of Ollama.Model</returns>
    public static async Task<Model[]> List()
    {
        var response = await GetRequest<Response.List>(Endpoints.LIST);
        return response.models;
    }
}

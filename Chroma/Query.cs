using Newtonsoft.Json;
using System.Threading.Tasks;

public static partial class ChromaDB
{
    /// <summary> Retrive the cloest entry </summary>
    public static async Task<string> Query(string collection_id, float[] embedding)
    {
        var request = new Request.Query(embedding, 1);
        string payload = JsonConvert.SerializeObject(request);
        var response = await PostRequest<Response.Query>(payload, Endpoints.QUERY.Replace("{id}", collection_id));
        return response.ids[0][0];
    }
}

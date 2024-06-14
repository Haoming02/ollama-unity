using Newtonsoft.Json;
using System.Threading.Tasks;

public static partial class ChromaDB
{
    /// <summary> Add a new entry to a collection </summary>
    public static async Task<bool> Add(string collection_id, string id, float[] embedding)
    {
        var request = new Request.Add(id, embedding);
        string payload = JsonConvert.SerializeObject(request);
        return await PostRequest<bool>(payload, Endpoints.ADD.Replace("{id}", collection_id));
    }

    /// <summary> Get the ID of an entry </summary>
    public static async Task<string> Get(string collection_id, string id)
    {
        var request = new Request.Get(id);
        string payload = JsonConvert.SerializeObject(request);
        var response = await PostRequest<Response.DataPoint>(payload, Endpoints.GET.Replace("{id}", collection_id));

        if (response.ids.Length == 1)
            return response.ids[0];
        else if (response.ids.Length == 0)
            return null;

        throw new System.IndexOutOfRangeException();
    }
}

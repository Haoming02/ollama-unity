using Newtonsoft.Json;
using System.Threading.Tasks;

public static partial class ChromaDB
{
    /// <summary> Return the number of collections in the database </summary>
    public static async Task<int> Count_Collections()
    {
        var response = await GetRequest<int>(Endpoints.COUNT_COLLECTIONS);
        return response;
    }

    /// <summary> Create a new collection with name </summary>
    public static async Task<string> Create_Collections(string name)
    {
        var request = new Request.Create_Collection(name);
        string payload = JsonConvert.SerializeObject(request);
        var response = await PostRequest<Response.Collection>(payload, Endpoints.COLLECTIONS);
        return response.id;
    }

    /// <summary> Get the ID of a collection with name </summary>
    public static async Task<string> Collections(string name)
    {
        var response = await GetRequest<Response.Collection>($"{Endpoints.COLLECTIONS}/{name}");
        return response.id;
    }

    /// <summary> Remove all collections <br/><b>Destructive!</b> </summary>
    public static async Task Reset()
    {
        await PostRequest<bool>(null, Endpoints.RESET);
    }
}

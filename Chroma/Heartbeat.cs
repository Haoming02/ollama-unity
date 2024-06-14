using System.Threading.Tasks;

public static partial class ChromaDB
{
    /// <summary> Sanity check to see if server is running </summary>
    public static async Task<long> Heartbeat()
    {
        var response = await GetRequest<Response.Heartbeat>(Endpoints.HEARTBEAT);
        return response.ns;
    }
}

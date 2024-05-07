using Newtonsoft.Json;
using System.Threading.Tasks;

public static partial class Ollama
{
    public static async Task<string> Chat(string input, string model = "llama3")
    {
        Request request = new Request(model, input, false);
        string payload = JsonConvert.SerializeObject(request);
        Response response = await SendRequest(payload);
        return response.response;
    }
}

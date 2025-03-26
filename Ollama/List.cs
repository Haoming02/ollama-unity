using System.Threading.Tasks;

namespace ollama
{
    public static partial class Ollama
    {
        /// <summary>
        /// Return a list of all locally available models <br/>
        /// Use <see cref="Model.ModelDetail.families">families</see> to determine if a model is multimodal
        /// </summary>
        /// <returns>an array of <see cref="Model">Ollama.Model</see></returns>
        public static async Task<Model[]> List()
        {
            var response = await GetRequest<Response.List>(Endpoints.LIST);
            return response.models;
        }
    }
}

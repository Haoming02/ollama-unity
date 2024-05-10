using UnityEngine;

public class OllamaEmbeddingDemo : MonoBehaviour
{
    async void Start()
    {
        var v1 = await Ollama.Embeddings("a cute dog");
        var v2 = await Ollama.Embeddings("a cute cat");
        var v3 = await Ollama.Embeddings("a tall skyscraper");

        var v12 = Ollama.CosineSimilarity(v1, v2);
        var v13 = Ollama.CosineSimilarity(v1, v3);

        Debug.Assert(v12 > v13);
        Debug.Log($"[Dog X Cat]: {v12} ; [Dog X Building]: {v13}");
    }
}

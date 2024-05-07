using UnityEngine;

public class OllamaDemo : MonoBehaviour
{
    async void Start()
    {
        var response = await Ollama.List();
        Debug.Log(string.Join(", ", response));
    }

    /// <summary>Called by UnityEngine.UI.InputField</summary>
    public async void OnSend(string input)
    {
        var response = await Ollama.Generate(input);
        Debug.Log(response);
    }
}

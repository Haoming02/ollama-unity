using UnityEngine;

public class ChatDemo : MonoBehaviour
{
    /// <summary>Called by UnityEngine.UI.InputField</summary>
    public async void OnSend(string input)
    {
        var response = await Ollama.Chat(input);
        Debug.Log(response);
    }
}
